using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Threading;

using static CAN_Loader.Globals;

namespace CAN_Loader
{
    class Usb
    {
        UsbDevice MyUsbDevice;
        UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x04D8, 0x0A30);
        UsbEndpointReader reader;
        UsbEndpointWriter writer;

        public Usb()
        {
            // Find and open the usb device.
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

            // If the device is open and ready
            if (MyUsbDevice == null) throw new Exception("Device Not Found.");

            // If this is a "whole" usb device (libusb-win32, linux libusb)
            // it will have an IUsbDevice interface. If not (WinUSB) the 
            // variable will be null indicating this is an interface of a 
            // device.
            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                // This is a "whole" USB device. Before it can be used, 
                // the desired configuration and interface must be selected.

                // Select config #1
                wholeUsbDevice.SetConfiguration(1);

                // Claim interface #0.
                wholeUsbDevice.ClaimInterface(0);
            }

            // open read endpoint 1.
            reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

            // open write endpoint 1.
            writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
        }

        public void TransferOut(byte[] buffer)
        {
            ErrorCode ec = ErrorCode.None;
            try
            {
                int bytesWritten;
                ec = writer.Write(buffer, 1000, out bytesWritten);
                if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
        }

        public void Receive()
        {
            byte[] packetId = new byte[4];
            uint tempID;
            int bytesReaden;
            byte[] readBuffer = new byte[19];

            while (true)
            {
                reader.Read(readBuffer, 50, out bytesReaden);
                Thread.Sleep(1);

                if (readBuffer[0] == dRECEIVE_MESSAGE)
                {
                    packetId[0] = readBuffer[1];
                    packetId[1] = readBuffer[2];
                    packetId[2] = readBuffer[3];
                    packetId[3] = readBuffer[4];
                    tempID = mchpID2CANid(packetId);

                    for (int i = 0; i < 19; i++)
                    {
                        packetBuffer[i] = readBuffer[i];
                    }

                    string hexValue = tempID.ToString("X");
                    Console.WriteLine("Recieved message ID:" + hexValue + "  Data: " + readBuffer[6]);
                }
            }
        }

        public UInt32 mchpID2CANid(byte[] mchpID)
        {
            UInt32 res = 0;
            if ((mchpID[3] & 0x08) == 0x08)
            {
                res = ((UInt32)mchpID[3] >> 3) & 0x1C;//0b00011100;
                res |= ((UInt32)mchpID[3] & 0x3);
                res |= (UInt32)mchpID[2] << 5;
                res *= 0x10000;
                //res += ((UInt32)mchpID[2] >>3)*0x1000000;
                res += (UInt32)mchpID[0] * 0x100;
                res += (UInt32)mchpID[1];
            }
            else
            {
                res = (UInt32)mchpID[2] * 0x100 + mchpID[3];
                res = res >> 5;
            }
            return res;
        }
    }
}
