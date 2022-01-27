using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Text;
using System.Threading;

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

        public void Recieve()
        {
            while (true)
            {
                int bytesReaden;
                byte[] readBuffer = new byte[19];
                reader.Read(readBuffer, 50, out bytesReaden);
                Thread.Sleep(1);
            }
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
    }
}
