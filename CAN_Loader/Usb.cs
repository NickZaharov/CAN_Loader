﻿using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Threading;

using static CAN_Loader.Globals;

namespace CAN_Loader
{
    class Usb
    {
        UsbDevice MyUsbDevice;
        UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x04d8, 0x0a30);
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
            int bytesReaden;
            byte[] readBuffer = new byte[19];
            while (true)
            {
                reader.Read(readBuffer, 50, out bytesReaden);
                if (readBuffer[0] == dRECEIVE_MESSAGE)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        packetId[i] = readBuffer[i + 1];
                    }
                    for (int i = readBuffer[5]; i < 8; i++)
                    {
                        readBuffer[i + 6] = 0;
                    }
                    gPacketID = mchpID2CANid(packetId);
                    gWordNumber = BitConverter.ToInt32(readBuffer, 10);
                    Console.WriteLine("Recieved message ID:" + gPacketID.ToString("X") + "  Data: " + readBuffer[6] + " " + readBuffer[7] + " " + readBuffer[8] + " " + readBuffer[9] + " " + readBuffer[10]
                         + " " + readBuffer[11] + " " + readBuffer[12] + " " + readBuffer[13] + " Number: " + BitConverter.ToInt32(readBuffer, 10));
                    for (int i = 0; i < 19; i++)
                    {
                        packetBuffer[i] = readBuffer[i];
                        readBuffer[i] = 0;
                    }
                    Thread.Sleep(1);
                }
                /*
                 if (readBuffer[6] != 250 && readBuffer[6] != 0 && readBuffer[0] != 255)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        packetId[i] = readBuffer[i + 1];
                    }
                    
                    gPacketID = mchpID2CANid(packetId);
                    gWordNumber = BitConverter.ToInt32(readBuffer, 10);
                    Console.WriteLine("Recieved message ID:" + gPacketID.ToString("X") + "  Data: " + readBuffer[6] + " " + readBuffer[7] + " " + readBuffer[8] + " " + readBuffer[9] + " " + readBuffer[10]
                         + " " + readBuffer[11] + " " + readBuffer[12] + " " + readBuffer[13] + " Number: " + BitConverter.ToInt32(readBuffer, 10));
                    for (int i = 0; i < 19; i++)
                    {
                        packetBuffer[i] = readBuffer[i];
                        readBuffer[i] = 0;
                    }
                }
                 */
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