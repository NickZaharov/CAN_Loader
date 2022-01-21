using System;
using System.Windows.Forms;

namespace CAN_Loader
{
    public partial class Form1 : Form
    {
        Usb usb;

		//Defines
        int dSPI_CAN_PACKET_SIZE = 19;
        int dSPI_CAN_PACKET_CHECKSUM_LOCATION = 18;
		//USB Commands
		byte dTRANSMIT_MESSAGE_EV = 0xA3;
		//////////////////////////////////////////////////
		public Form1()
        {
            InitializeComponent();
            usb = new Usb();
        }

        private void SendPacket_Click(object sender, EventArgs e)
        {
            byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];
			bool extendedFlag = true;

			uint tempID = 7;
			byte tempSIDH = 0;
			byte tempSIDL = 0;
			byte tempEIDH = 0;
			byte tempEIDL = 0;
			byte tempDLC = 8;

			byte CheckSum = 0;

			//Transmit single message
			//////////////////////////////////////////////////////////////////
			

			//break out tempID into SIDH / SIDL / EIDH / EIDL
			breakOutIDintoHex(extendedFlag, tempID, tempSIDH, tempSIDL, tempEIDH, tempEIDL);

			//Hard code the command for now... may need set this along with the period and repeat later
			OutputPacketBuffer[0] = dTRANSMIT_MESSAGE_EV; //Command
			OutputPacketBuffer[1] = tempEIDH;//0x01; //EIDH
			OutputPacketBuffer[2] = tempEIDL;//0x00; //EIDL
			OutputPacketBuffer[3] = tempSIDH;//0x00; //SIDH
			OutputPacketBuffer[4] = 244;//0x00; //SIDL
			OutputPacketBuffer[5] = tempDLC;//0x08; //tempDLC

			//CAN data
			OutputPacketBuffer[6] = 2;
			OutputPacketBuffer[7] = 2;
			OutputPacketBuffer[8] = 2;
			OutputPacketBuffer[9] = 2;
			OutputPacketBuffer[10] = 1;
			OutputPacketBuffer[11] = 1;
			OutputPacketBuffer[12] = 1;
			OutputPacketBuffer[13] = 2;

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

			//usb.TransferOut(OutputPacketBuffer, dSPI_CAN_PACKET_SIZE);
			usb.TransferOut(OutputPacketBuffer);
        }

		void breakOutIDintoHex(bool passedInExtendedFlag, uint passedInID, uint passedInSIDH, uint passedInSIDL, uint passedInEIDH, uint passedInEIDL)
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
				passedInEIDL = 0xFF & tempPassedInID; //CAN_extendedLo_ID_TX1 = &HFF And CAN_UserEnter_ID_TX1
				tempPassedInID = tempPassedInID >> 8; //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 >> 8

				//EIDH
				passedInEIDH = 0xFF & tempPassedInID; //CAN_extendedHi_ID_TX1 = &HFF And CAN_UserEnter_ID_TX1
				tempPassedInID = tempPassedInID >> 8;  //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 >> 8

				//SIDL
				//push back 5 and or it
				wipSIDL = 0x03 & tempPassedInID;
				tempPassedInID = tempPassedInID << 3; //CAN_UserEnter_ID_TX1 = CAN_UserEnter_ID_TX1 << 3
				wipSIDL = (0xE0 & tempPassedInID) + wipSIDL;
				wipSIDL = wipSIDL + 0x08; // TEMP_CAN_standardLo_ID_TX1 = TEMP_CAN_standardLo_ID_TX1 + &H8
				passedInSIDL = 0xEB & wipSIDL; //CAN_standardLo_ID_TX1 = &HEB And TEMP_CAN_standardLo_ID_TX1

				//SIDH
				tempPassedInID = tempPassedInID >> 8;
				passedInSIDH = 0xFF & tempPassedInID;
			}
			else
			{
				////Standard message (11 bits)
				////EIDH = 0 + EIDL = 0 + SIDH + upper three bits SIDL (3rd bit needs to be clear)
				////1111 1111 111
				passedInEIDH = 0;
				passedInEIDL = 0;
				tempPassedInID = tempPassedInID << 5;
				passedInSIDL = 0xFF & tempPassedInID;
				tempPassedInID = tempPassedInID >> 8;
				passedInSIDH = 0xFF & tempPassedInID;
			}
		}
	}
}
