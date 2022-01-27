using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static CAN_Loader.Constants;

namespace CAN_Loader
{
	public partial class Form1 : Form
	{
		Usb usb;
		string FilePath;


		public Form1()
		{
			InitializeComponent();
			usb = new Usb();
		}

		void LoadPLC()
		{
			Console.WriteLine("======LOAD PLC START=======\n");
			JumpToBootloader();
			WriteFlash();
			JumpToApp();
			StopPLC();
			Console.WriteLine("======LOAD PLC FINISH=======\n");
		}

		void JumpToBootloader()
		{
			SendCmd(_CMD_RESET);
			Thread.Sleep(200);
		}

		void WriteFlash()
		{
			byte[] tmp = new byte[8];

			SendCmd(_CMD_FLASH_EARSE);
			SendCmd(_CMD_FLASH_WRITE_START);

			FileInfo fileInf = new FileInfo(FilePath);
			if (fileInf.Exists)
			{
				using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
				{
					// пока не достигнут конец файла считываем по 2 байта
					while (reader.BaseStream.Position != reader.BaseStream.Length)
					{
						tmp = reader.ReadBytes(8);
						SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, tmp);
					}
					SendCmd(_CMD_FLASH_WRITE_FINISH);
				}
			}
			else
			{

			}
		}

		void JumpToApp()
		{
			Console.WriteLine("========jump to app========\n");
			SendCmd(_CMD_BOOT_LOCK);
			SendCmd(_CMD_JUMP_TO_APP); 
			Thread.Sleep(5000);
		}

		void StopPLC()
		{
			SendCmd(_CMD_STOP_PLC);
		}

		void SendCmdWithData(uint cmd, byte[] data)
		{
			byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];
			bool extendedFlag = true;

			uint tempID = (0xFF00 | cmd) << 8;
			byte tempSIDH = 0;
			byte tempSIDL = 0;
			byte tempEIDH = 0;
			byte tempEIDL = 0;
			byte tempDLC = 8;
			byte CheckSum = 0;

			//break out tempID into SIDH / SIDL / EIDH / EIDL
			breakOutIDintoHex(extendedFlag, tempID, ref tempSIDH, ref tempSIDL, ref tempEIDH, ref tempEIDL);

			//Hard code the command for now... may need set this along with the period and repeat later
			OutputPacketBuffer[0] = dTRANSMIT_MESSAGE_EV; //Command
			OutputPacketBuffer[1] = tempEIDH;//0x01; //EIDH
			OutputPacketBuffer[2] = tempEIDL;//0x00; //EIDL
			OutputPacketBuffer[3] = tempSIDH;//0x00; //SIDH
			OutputPacketBuffer[4] = tempSIDL;//0x00; //SIDL
			OutputPacketBuffer[5] = tempDLC;//0x08; //tempDLC

			//CAN data
			OutputPacketBuffer[6] = data[0];
			OutputPacketBuffer[7] = data[1];
			OutputPacketBuffer[8] = data[2];
			OutputPacketBuffer[9] = data[3];
			OutputPacketBuffer[10] = data[4];
			OutputPacketBuffer[11] = data[5];
			OutputPacketBuffer[12] = data[6];
			OutputPacketBuffer[13] = data[7];
			//////////
			OutputPacketBuffer[14] = 0;
			OutputPacketBuffer[15] = 0;//tempPeriodHigh;	Not used for a single shot
			OutputPacketBuffer[16] = 0;//tempPeriodLow;	Not used for a single shot
			OutputPacketBuffer[17] = 0;//tempRepeat;		Not used for a single shot

			for (int i = 0; i < dSPI_CAN_PACKET_SIZE; i++) //2 - Start checksum at Command, 19 last byte of timestamp, 20 is checksum byte
			{
				CheckSum += OutputPacketBuffer[i];
			}
			//Checksum byte
			OutputPacketBuffer[dSPI_CAN_PACKET_CHECKSUM_LOCATION] = CheckSum;

			//Transmit single message
			usb.TransferOut(OutputPacketBuffer);
		}

		void SendCmd(uint cmd)
		{
			byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];
			bool extendedFlag = true;

			uint tempID = (0xFF00 | cmd) << 8;
			
			byte tempSIDH = 0;
			byte tempSIDL = 0;
			byte tempEIDH = 0;
			byte tempEIDL = 0;
			byte tempDLC = 8;
			byte CheckSum = 0;

			//break out tempID into SIDH / SIDL / EIDH / EIDL
			breakOutIDintoHex(extendedFlag, tempID, ref tempSIDH, ref tempSIDL, ref tempEIDH, ref tempEIDL);

			//Hard code the command for now... may need set this along with the period and repeat later
			OutputPacketBuffer[0] = dTRANSMIT_MESSAGE_EV; //Command
			OutputPacketBuffer[1] = tempEIDH;//0x01; //EIDH
			OutputPacketBuffer[2] = tempEIDL;//0x00; //EIDL
			OutputPacketBuffer[3] = tempSIDH;//0x00; //SIDH
			OutputPacketBuffer[4] = tempSIDL;//0x00; //SIDL
			OutputPacketBuffer[5] = tempDLC;//0x08; //tempDLC

			//CAN data
			OutputPacketBuffer[6] = 0;
			OutputPacketBuffer[7] = 0;
			OutputPacketBuffer[8] = 0;
			OutputPacketBuffer[9] = 0;
			OutputPacketBuffer[10] = 0;
			OutputPacketBuffer[11] = 0;
			OutputPacketBuffer[12] = 0;
			OutputPacketBuffer[13] = 0;
			//////////
			OutputPacketBuffer[14] = 0;
			OutputPacketBuffer[15] = 0;//tempPeriodHigh;	Not used for a single shot
			OutputPacketBuffer[16] = 0;//tempPeriodLow;	Not used for a single shot
			OutputPacketBuffer[17] = 0;//tempRepeat;		Not used for a single shot

			for (int i = 0; i < dSPI_CAN_PACKET_SIZE; i++) //2 - Start checksum at Command, 19 last byte of timestamp, 20 is checksum byte
			{
				CheckSum += OutputPacketBuffer[i];
			}
			//Checksum byte
			OutputPacketBuffer[dSPI_CAN_PACKET_CHECKSUM_LOCATION] = CheckSum;

			//Transmit single message
			usb.TransferOut(OutputPacketBuffer);
		}

		void breakOutIDintoHex(bool passedInExtendedFlag, uint passedInID, ref byte passedInSIDH, ref byte passedInSIDL, ref byte passedInEIDH, ref byte passedInEIDL)
		{
			uint tempPassedInID;
			uint wipSIDL;

			tempPassedInID = passedInID;

			if (passedInExtendedFlag == true)
			{
				////Extended messsage (29bits)
				////SIDH + upper three bits SIDL (3rd bit needs to be set) lower two bits + EIDH + EIDL
				////CAN_standardHi_ID + CAN_standardLo_ID + CAN_extendedHi_ID + CAN_extendedLo_ID
				////Set the SIDL bit and convert as if a extended message
				//		   // sHi       / sLo    / eHi       / eLo
				//		   // 1111 1111 / 111 11 / 1111 1111 / 1111 1111
				//            00001111 222x3x33 44445555 66667777
				//            sidh       sidl      eidh      eidl

				//EIDL
				passedInEIDL = (byte)(0xFF & tempPassedInID); //CAN_extendedLo_ID_TX1 = &HFF And CAN_UserEnter_ID_TX1
				tempPassedInID = tempPassedInID >> 8; //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 >> 8

				//EIDH
				passedInEIDH = (byte)(0xFF & tempPassedInID); //CAN_extendedHi_ID_TX1 = &HFF And CAN_UserEnter_ID_TX1
				tempPassedInID = tempPassedInID >> 8;  //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 >> 8

				//SIDL
				//push back 5 and or it
				wipSIDL = 0x03 & tempPassedInID;
				tempPassedInID = tempPassedInID << 3; //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 << 3
				wipSIDL = (0xE0 & tempPassedInID) + wipSIDL;
				wipSIDL = wipSIDL + 0x08; // TEMP_CAN_standardLo_ID_TX1 = TEMP_CAN_standardLo_ID_TX1 + &H8
				passedInSIDL = (byte)(0xEB & wipSIDL); //CAN_standardLo_ID_TX1 = &HEB And TEMP_CAN_standardLo_ID_TX1

				//SIDH
				tempPassedInID = tempPassedInID >> 8;
				passedInSIDH = (byte)(0xFF & tempPassedInID);
			}
			else
			{
				////Standard message (11 bits)
				////EIDH = 0 + EIDL = 0 + SIDH + upper three bits SIDL (3rd bit needs to be clear)
				////1111 1111 111
				passedInEIDH = 0;
				passedInEIDL = 0;
				tempPassedInID = tempPassedInID << 5;
				passedInSIDL = (byte)(0xFF & tempPassedInID);
				tempPassedInID = tempPassedInID >> 8;
				passedInSIDH = (byte)(0xFF & tempPassedInID);
			}
		}

		private void btn_fileDialog_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";
				openFileDialog.Filter = "bin files (*.bin) | *.bin";
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					FilePath = openFileDialog.FileName;
				}
			}
		}

        private void button1_Click(object sender, EventArgs e)
        {
			int i = 1;
			while (i > 0)
			{
				if(i == 1)
                {
					usb.Recieve();
					i++;
                }
				SendCmd(_CMD_ECHO);
				Thread.Sleep(100);
			}
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
			Task task = new Task(usb.Recieve);
			task.Start();
			LoadPLC();
		}
    }
}
