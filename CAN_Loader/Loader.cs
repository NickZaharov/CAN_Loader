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
		private readonly CanMicrochip _can;

		Stopwatch stopwatch = new Stopwatch();

		delegate void ChangeProgress(string text);

		public Loader(CanMicrochip can)
		{
			_can = can;
		}
		public void LoadPLC(string FilePath, ProgressBar progressBar)
		{
			Console.WriteLine("======LOAD PLC START=======\n");
			JumpToBootloader();
			WriteFlash(FilePath, progressBar);
			JumpToApp();
			StopPLC();
			Console.WriteLine("======LOAD PLC FINISH=======\n");
		}

		void JumpToBootloader()
		{
			packetBuffer[6] = 0;
			_can.SendCmd(_CMD_RESET);
			while (packetBuffer[6] != _OK) { }
			Thread.Sleep(5000);
		}

		void WriteFlash(string FilePath, ProgressBar progressBar)
		{
			byte[] tmp = new byte[8];

			packetBuffer[6] = 0;
			_can.SendCmd(_CMD_FLASH_EARSE);
			while (packetBuffer[6] != _OK) { }

			_can.SendCmd(_CMD_FLASH_WRITE_START);

			FileInfo fileInf = new FileInfo(FilePath);
			if (fileInf.Exists)
			{
				int i = 0, progress = 0;
				using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
				{
					// пока не достигнут конец файла считываем по 4 байта
					while (reader.BaseStream.Position != reader.BaseStream.Length)
					{
						tmp = reader.ReadBytes(4);
						i += 4;

						stopwatch.Start();
						packetBuffer[6] = 0;
						_can.SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, tmp);
						while (packetBuffer[6] != _OK) { }
						stopwatch.Stop();
						Console.WriteLine(stopwatch.ElapsedMilliseconds);
						stopwatch.Reset();

						progressBar.Value = (int)(i / (float)fileInf.Length * 100);
						if (progressBar.Value > progress)
						{
							progress = progressBar.Value;
							/*label1.Text = progress.ToString() + "%";
							label1.Update();
							Form1.label1.Invoke(new Del((s) => label1.Text = s), "newText");*/

						}
					}
					_can.SendCmd(_CMD_FLASH_WRITE_FINISH);
				}
			}
			else
			{

			}
		}

		void JumpToApp()
		{
			Console.WriteLine("========jump to app========\n");
			_can.SendCmd(_CMD_BOOT_LOCK);
			_can.SendCmd(_CMD_JUMP_TO_APP);
			Thread.Sleep(5000);
		}

		void StopPLC()
		{
			_can.SendCmd(_CMD_STOP_PLC);
		}
	}
}
