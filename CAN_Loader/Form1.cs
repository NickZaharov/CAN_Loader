using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using static CAN_Loader.Globals;

namespace CAN_Loader
{
	public partial class Form1 : Form
	{
		Loader loader;
		CanMicrochip can;
		string FilePath;
		
		public Form1()
		{
			InitializeComponent();

			Usb usb = new Usb();
			can = new CanMicrochip(usb);
			Loader loader = new Loader(can);

			Task receiveTask = new Task(usb.Receive);
			receiveTask.Start();
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
			can.SendCmd(_CMD_INFO);
		}

        private void btn_Load_Click(object sender, EventArgs e)
        {
			loader.LoadPLC(FilePath, progressBar1);
		}
    }
}
