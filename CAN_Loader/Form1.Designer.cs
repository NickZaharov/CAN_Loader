
namespace CAN_Loader
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.SendPacket = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SendPacket
            // 
            this.SendPacket.Location = new System.Drawing.Point(88, 77);
            this.SendPacket.Name = "SendPacket";
            this.SendPacket.Size = new System.Drawing.Size(168, 39);
            this.SendPacket.TabIndex = 0;
            this.SendPacket.Text = "Отправить посылку";
            this.SendPacket.UseVisualStyleBackColor = true;
            this.SendPacket.Click += new System.EventHandler(this.SendPacket_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.SendPacket);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SendPacket;
    }
}

