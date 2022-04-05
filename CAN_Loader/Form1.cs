using System;
using System.Windows.Forms;

using static CAN_Loader.Globals;

namespace CAN_Loader
{
    public partial class Form1 : Form
    {
        Loader loader;
        string filePath;
        string openFile;

        public Form1()
        {
            InitializeComponent();
            loader = new Loader(progressBar1, label1);
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

        private void btn_Load_Click(object sender, EventArgs e)
        {
            if (filePath == null) MessageBox.Show("Не выбран файл для прошивки");
            else
            {
                btn_fileDialog.Enabled = false;
                btn_Load.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                ///Запуск прошивки
                if (!loader.LoadPLC(filePath))
                {
                    MessageBox.Show("Проверьте подключения!");
                }
                else
                {
                    textBox1.Text += Environment.NewLine + "Файл успешно загружен: " + filePath + Environment.NewLine + "------------------";
                }
                this.Cursor = Cursors.Default;
                btn_fileDialog.Enabled = true;
                btn_Load.Enabled = true;
            }
        }
    }
}