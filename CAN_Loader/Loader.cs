using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static CAN_Loader.Globals;

namespace CAN_Loader
{
    class Loader
    {
		private readonly CanMicrochip can;
		private readonly Usb usb;
		Stopwatch stopwatch = new Stopwatch();
		ProgressBar progressBar;
		Label progressLabel;

		public Loader(ProgressBar pb, Label pl)
		{
			usb = new Usb();
			can = new CanMicrochip(usb);
			progressBar = pb;
			progressLabel = pl;
		}

		public bool LoadPLC(string FilePath)
		{
			if (!usb.Usb_Connect())
			{
				return false;
			}
            else
            {
				Console.WriteLine("======LOAD PLC START=======\n");
				JumpToBootloader();
				WriteFlash(FilePath);
				JumpToApp();
				StopPLC();
				Console.WriteLine("======LOAD PLC FINISH=======\n");
			}
			return true;
		}

		 public void JumpToBootloader()
		{
			packetBuffer[6] = 0;
			can.SendCmd(_CMD_RESET);
			while (packetBuffer[6] != _OK) { Thread.Sleep(5); }
			Thread.Sleep(5000);
		}

		void WriteFlash(string FilePath)
		{
			int error = 0;
			byte[] data = new byte[8];
			byte[] tmp;
			packetBuffer[6] = 0;
			can.SendCmd(_CMD_FLASH_EARSE);
			while (packetBuffer[6] != _OK) { Thread.Sleep(5); }
			packetBuffer[6] = 0;
			can.SendCmd(_CMD_FLASH_WRITE_START);
			while (packetBuffer[6] != _OK) { Thread.Sleep(5); }
			FileInfo fileInf = new FileInfo(FilePath);
			if (fileInf.Exists)
			{
				int bytesReaden = 0, progress = 0;
				int wordCounter = 0;
				using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
				{
					// пока не достигнут конец файла считываем по 4 байта
					while (reader.BaseStream.Position != reader.BaseStream.Length)
					{
						tmp = reader.ReadBytes(4);
						for (int i = 0; i < 4; i++) data[i] = tmp[i];
						bytesReaden += 4;
						wordCounter++;
						byte[] numInBytes = BitConverter.GetBytes(wordCounter);
						for (int i = 0; i < 4; i++) data[i + 4] = numInBytes[i];

						//stopwatch.Start();

						can.ClearBuffer();
						gPacketID = 0;
						int timeout = 0;
						can.SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, data);
						while (gPacketID == 0) 
						{
							Thread.Sleep(1);
							timeout++;
							if (timeout % 
								100 == 0)
                            {
								error++;
								can.SendCmd(CMD_GET_LAST_RX_DATA);
								while (true)
								{
									Thread.Sleep(1);
									timeout++;
									if (timeout % 200 == 0)
									{
										break;
									}
									if(gWordNumber != 0)
                                    {
										if (wordCounter != gWordNumber)
										{
											gPacketID = 0;
											can.SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, data);
										}
										gWordNumber = 0;
										break;
									}
								}
							}
						}

                        //stopwatch.Stop();
                        //Console.WriteLine(stopwatch.ElapsedMilliseconds);
                        //stopwatch.Reset();

                        progressBar.Value = (int)(bytesReaden / (float)fileInf.Length * 100);
						if (progressBar.Value > progress)
						{
							progress = progressBar.Value;
							progressLabel.Text = progress.ToString() + "%";
							progressLabel.Update();

						}
						Thread.Sleep(2);
					}
					can.SendCmd(_CMD_FLASH_WRITE_FINISH);
					Console.WriteLine(error);
				}
			}
			else
			{

			}
		}

		void JumpToApp()
		{
			Console.WriteLine("========jump to app========\n");
			can.SendCmd(_CMD_BOOT_LOCK);
			can.SendCmd(_CMD_JUMP_TO_APP);
			Thread.Sleep(5000);
		}

		void StopPLC()
		{
			can.SendCmd(_CMD_STOP_PLC);
		}
	}
}
