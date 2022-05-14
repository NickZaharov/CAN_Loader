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

        ProgressBar progressBar;
        Label progressLabel;
        TextBox textBox;
        bool checkedBitrate = false;

        Stopwatch stopwatch = new Stopwatch();
        Stopwatch timer = new Stopwatch();

        public Loader(ProgressBar pb, Label pl, TextBox tb)
        {
            usb = new Usb();
            can = new CanMicrochip(usb);
            progressBar = pb;
            progressLabel = pl;
            textBox = tb;
        }

        public void LoadPLC()
        {
            if (GetStatus() < 1)
            {
                gLoaderResponse = status_SILENCE;
                return;
            }
            if (!usb.Usb_Connect())
            {
                gLoaderResponse = status_ERROR;
                return;
            }
            else
            {
                textBox.Invoke(new Action(() => textBox.Text += "------------------" + Environment.NewLine + "Начало загрузки..." + Environment.NewLine));
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                Console.WriteLine("======LOAD PLC START=======\n");
                JumpToBootloader();
                if (!WriteFlash())
                {
                    gLoaderResponse = status_DISCONNECT;
                    usb.Usb_Disconnect();
                    return;
                }
                JumpToApp();
                StopPLC();
                Console.WriteLine("======LOAD PLC FINISH=======\n");
            }
            usb.Usb_Disconnect();
            gLoaderResponse = status_OK;
        }

        public void JumpToBootloader()
        {
            packetBuffer[6] = 0;
            can.SendCmd(_CMD_RESET);
            Thread.Sleep(5000);
            while (packetBuffer[6] != _OK) { Thread.Sleep(20); }
        }

        bool WriteFlash()
        {
            byte[] data = new byte[8];
            byte[] tmp;

            packetBuffer[6] = 0;
            can.SendCmd(_CMD_FLASH_EARSE);
            while (packetBuffer[6] != _OK) { Thread.Sleep(20); }

            packetBuffer[6] = 0;
            can.SendCmd(_CMD_FLASH_WRITE_START);
            while (packetBuffer[6] != _OK) { Thread.Sleep(1); }

            FileInfo fileInf = new FileInfo(filePath);
            if (fileInf.Exists)
            {
                int bytesReaden = 0, progress = -1, wordCounter = 0;
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    // пока не достигнут конец файла считываем по 4 байта
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        stopwatch.Start();
                        int timeout = 0;
                        tmp = reader.ReadBytes(4);
                        for (int i = 0; i < 4; i++)
                            data[i] = tmp[i];

                        bytesReaden += 4;
                        wordCounter++;
                        byte[] numInBytes = BitConverter.GetBytes(wordCounter);
                        for (int i = 0; i < 4; i++)
                            data[i + 4] = numInBytes[i];

                        can.ClearBuffer();
                        gPacketID = 0;
                        can.SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, data);
                        while (gPacketID == 0)
                        {
                            Thread.Sleep(1);
                            timeout++;
                            if (timeout > 500)
                            {
                                can.SendCmd(CMD_GET_LAST_RX_DATA);
                                while (gPacketID == 0)
                                {
                                    Thread.Sleep(1);
                                    timeout++;
                                    if (timeout > 1000)
                                    {
                                        return false;
                                    }
                                    if (gWordNumber != 0)
                                    {
                                        timer.Reset();
                                        if (wordCounter != gWordNumber)
                                            can.SendCmdWithData(_CMD_FLASH_WRITE_NEXT_WORD, data);
                                        break;
                                    }
                                }
                            }
                        }
                        progressBar.Invoke(new Action(() => progressBar.Value = (int)(bytesReaden / (float)fileInf.Length * 100)));
                        if (progressBar.Value > progress)
                        {
                            textBox.Invoke(new Action(() => textBox.Text += stopwatch.ElapsedMilliseconds + Environment.NewLine));
                            progress = progressBar.Value;
                            progressLabel.Invoke(new Action(() => progressLabel.Text = progress.ToString() + "%"));
                        }
                        Thread.Sleep(2);
                        Console.WriteLine(stopwatch.ElapsedMilliseconds);
                        stopwatch.Reset();
                    }
                    can.SendCmd(_CMD_FLASH_WRITE_FINISH);
                }
            }
            return true;
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

        public int GetStatus()
        {
            int timeout = 0;
            if (!usb.Usb_Connect())
                return status_ERROR;
            if (!checkedBitrate)
            {
                can.ChangeBitRate(250);
                checkedBitrate = true;
            }
            can.ClearBuffer();
            can.SendCmd(_CMD_INFO);
            while (packetBuffer[6] == 0)
            {
                Thread.Sleep(5);
                timeout++;
                if (timeout > 20)
                {
                    usb.Usb_Disconnect();
                    return status_SILENCE;
                }
            }
            usb.Usb_Disconnect();
            if (packetBuffer[7] == _HOST_BOOTLOADER_ID) return status_InLoader;
            if (packetBuffer[7] == _HOST_PROGRAM_ID) return status_InProgram;
            else return status_SILENCE;
        }
    }
}
