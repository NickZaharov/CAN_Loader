using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static CAN_Loader.Globals;
using Peak.Can.Basic;
using System.Text;

namespace CAN_Loader
{
    class Loader
    {
        ProgressBar progressBar;
        Label progressLabel;
        TextBox textBox;
        PCANСustom pcan = new PCANСustom();

        Stopwatch stopwatch = new Stopwatch();

        public Loader(ProgressBar pb, Label pl, TextBox tb)
        {
            progressBar = pb;
            progressLabel = pl;
            textBox = tb;
            pcan.PCANInitialize();
        }

        public void LoadPLC()
        {
            textBox.Invoke(new Action(() => textBox.Text += "------------------" + Environment.NewLine + "Начало загрузки..." + Environment.NewLine));

            Console.WriteLine("======LOAD PLC START=======\n");
            if (!JumpToBootloader())
            {
                gLoaderResponse = status_DISCONNECT;
                return;
            }
            if (!WriteFlash())
            {
                gLoaderResponse = status_DISCONNECT;
                return;
            }
            JumpToApp();
            StopPLC();
            Console.WriteLine("======LOAD PLC FINISH=======\n");
            gLoaderResponse = status_OK;
        }

        public bool JumpToBootloader()
        {
            pcan.WriteMessage(_CMD_RESET);
            Thread.Sleep(5000);
            if (!pcan.ReadMessage())
                return false;
            return true;
        }

        bool WriteFlash()
        {
            byte[] data = new byte[8];
            byte[] tmp;

            pcan.WriteMessage(_CMD_FLASH_EARSE);
            if (!pcan.ReadMessage())
                return false;

            pcan.WriteMessage(_CMD_FLASH_WRITE_START);
            if (!pcan.ReadMessage())
                return false;

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

                        tmp = reader.ReadBytes(4);
                        for (int i = 0; i < 4; i++)
                            data[i] = tmp[i];

                        bytesReaden += 4;
                        wordCounter++;
                        byte[] numInBytes = BitConverter.GetBytes(wordCounter);
                        for (int i = 0; i < 4; i++)
                            data[i + 4] = numInBytes[i];

                        pcan.WriteMessage(_CMD_FLASH_WRITE_NEXT_WORD, data);
                        if (!pcan.ReadMessage())
                        {
                            pcan.WriteMessage(CMD_GET_LAST_RX_DATA);
                            if (!pcan.ReadMessage())
                                return false;
                            if (gWordNumber != 0)
                            {
                                if (wordCounter != gWordNumber)
                                    pcan.WriteMessage(_CMD_FLASH_WRITE_NEXT_WORD);
                            }
                        }

                        progressBar.Invoke(new Action(() => progressBar.Value = (int)(bytesReaden / (float)fileInf.Length * 100)));
                        if (progressBar.Value > progress)
                        {
                            progress = progressBar.Value;
                            progressLabel.Invoke(new Action(() => progressLabel.Text = progress.ToString() + "%"));
                        }

                        Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
                        stopwatch.Reset();
                    }
                    pcan.WriteMessage(_CMD_FLASH_WRITE_FINISH);
                }
            }
            return true;
        }

        void JumpToApp()
        {
            Console.WriteLine("========jump to app========\n");
            pcan.WriteMessage(_CMD_BOOT_LOCK);
            pcan.WriteMessage(_CMD_JUMP_TO_APP);
            Thread.Sleep(5000);
        }

        void StopPLC() => pcan.WriteMessage(_CMD_JUMP_TO_APP);

        public int GetStatus()
        {
            
            pcan.WriteMessage(_CMD_INFO);
            if (!pcan.ReadMessage())
                return status_SILENCE;
            if (pcan.MsgBuffer.DATA[1] == _HOST_BOOTLOADER_ID) return status_InLoader;
            if (pcan.MsgBuffer.DATA[1] == _HOST_PROGRAM_ID) return status_InProgram;
            else return status_SILENCE;
        }
     }
}

