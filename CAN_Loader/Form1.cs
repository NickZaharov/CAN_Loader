using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using static CAN_Loader.Globals;

namespace CAN_Loader
{
    public partial class Form1 : Form
    {
        Loader loader;
        string openFile;

        public Form1()
        {
            InitializeComponent();
            loader = new Loader(progressBar1, label1, textBox1);
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
                    filePath = openFileDialog.FileName;
                    if (filePath != openFile)
                    {
                        textBox1.Text += "Выбран файл: " + filePath + Environment.NewLine;
                        openFile = filePath;
                    }
                    else MessageBox.Show("Данный файл уже выбран");
                }
            } 
        }

        private async void btn_Load_Click(object sender, EventArgs e)
        {
            if (filePath == null) MessageBox.Show("Не выбран файл для прошивки");
            else
            {
                btn_fileDialog.Enabled = false;
                btn_Load.Enabled = false;
                btn_Check.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                ///Запуск прошивки
                await Task.Run(loader.LoadPLC);
                //Результат выполнения
                if (gLoaderResponse == status_ERROR)
                {
                    MessageBox.Show("Проверьте подключения!");
                }
                else if (gLoaderResponse == status_SILENCE)
                {
                    MessageBox.Show("Устройство не отвечает");
                }
                else if (gLoaderResponse == status_DISCONNECT)
                {
                    MessageBox.Show("Превышено время ожидания, возможно произошел обрыв соединения!");
                    textBox1.Text += "Файл не был загружен, проверьте подключения и попробуйте снова" + Environment.NewLine + "------------------" + Environment.NewLine;
                }
                else
                {
                    textBox1.Text += Environment.NewLine + "Файл успешно загружен: " + filePath + Environment.NewLine + "------------------" + Environment.NewLine;
                }
                this.Cursor = Cursors.Default;
                btn_fileDialog.Enabled = true;
                btn_Load.Enabled = true;
                btn_Check.Enabled = true;
            }
        }

        private void btn_Check_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            int status = loader.GetStatus();
            if(status < 1)
                status = loader.GetStatus();
            switch (status)
            {
                case status_ERROR:
                    MessageBox.Show("Проверьте подключения!");
                    break;
                case status_InLoader:
                    textBox1.Text += "Статус: в загрузчике (программа отсутствует)" + Environment.NewLine;
                    break;
                case status_InProgram:
                    textBox1.Text += "Статус: программа загружена" + Environment.NewLine;
                    break;
                case status_SILENCE:
                    textBox1.Text += "Статус: устройство не отвечает" + Environment.NewLine;
                    break;
            }
            this.Cursor = Cursors.Default;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
            textBox1.Refresh();
        }
    }
}