﻿using System;
using System.Threading;
using static CAN_Loader.Globals;

namespace CAN_Loader
{
    class CanMicrochip
    {
		readonly Usb _usb;

        public CanMicrochip(Usb usb) => _usb = usb;

		public void SendCmd(uint cmd)
		{
			byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];
			bool extendedFlag = true;

			uint tempID = (0xFF00 | cmd) << 8;

			//break out tempID into SIDH / SIDL / EIDH / EIDL
			BreakOutIDintoHex(extendedFlag, tempID, out byte tempSIDH, out byte tempSIDL, out byte tempEIDH, out byte tempEIDL);

			//Hard code the command for now... may need set this along with the period and repeat later
			OutputPacketBuffer[0] = dTRANSMIT_MESSAGE_EV;//Command
			OutputPacketBuffer[1] = tempEIDH;//EIDH
			OutputPacketBuffer[2] = tempEIDL;//EIDL
			OutputPacketBuffer[3] = tempSIDH;//SIDH
			OutputPacketBuffer[4] = tempSIDL;//SIDL
			OutputPacketBuffer[5] = 0;//DLC

			//Checksum byte
			ComputeChecksum(ref OutputPacketBuffer);

			//Transmit single message
			_usb.TransferOut(OutputPacketBuffer);
		}

		public void SendCmdWithData(uint cmd, byte[] data)
		{
			byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];
			bool extendedFlag = true;

			uint tempID = (0xFF00 | cmd) << 8;

			//break out tempID into SIDH / SIDL / EIDH / EIDL
			BreakOutIDintoHex(extendedFlag, tempID, out byte tempSIDH, out byte tempSIDL, out byte tempEIDH, out byte tempEIDL);

			//Hard code the command for now... may need set this along with the period and repeat later
			OutputPacketBuffer[0] = dTRANSMIT_MESSAGE_EV;//Command
			OutputPacketBuffer[1] = tempEIDH;//EIDH
			OutputPacketBuffer[2] = tempEIDL;//EIDL
			OutputPacketBuffer[3] = tempSIDH;//SIDH
			OutputPacketBuffer[4] = tempSIDL;//SIDL
			OutputPacketBuffer[5] = (byte)data.Length;//DLC

            //CAN data
            for(int i = 0; i < data.Length; i++)
            {
				OutputPacketBuffer[6 + i] = data[i];
			}

			ComputeChecksum(ref OutputPacketBuffer);

			_usb.TransferOut(OutputPacketBuffer);
		}
		
		public void ChangeBitRate(ushort bitRate)
        {
            byte[] OutputPacketBuffer = new byte[dSPI_CAN_PACKET_SIZE];

            OutputPacketBuffer[0] = dCHANGE_BIT_RATE;//Command
            OutputPacketBuffer[1] = (byte)((ushort)bitRate >> 8);
            OutputPacketBuffer[2] = (byte)bitRate;

			ComputeChecksum(ref OutputPacketBuffer);

			_usb.TransferOut(OutputPacketBuffer);
			Thread.Sleep(1000);
		}

		void ComputeChecksum(ref byte[] Buffer)
        {
			byte checkSum = 0;
			for (int i = 0; i < dSPI_CAN_PACKET_SIZE; i++)
			{
				checkSum += Buffer[i];
			}
			Buffer[dSPI_CAN_PACKET_CHECKSUM_LOCATION] = checkSum;
		}

		void BreakOutIDintoHex(bool passedInExtendedFlag, uint passedInID, out byte passedInSIDH, out byte passedInSIDL, out byte passedInEIDH, out byte passedInEIDL)
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

		public void ClearBuffer()
        {
			for (int i = 0; i < 19; i++) packetBuffer[i] = 0;
        }
	}
}