
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
            this.btn_fileDialog = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_Load = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_fileDialog
            // 
            this.btn_fileDialog.Location = new System.Drawing.Point(75, 161);
            this.btn_fileDialog.Name = "btn_fileDialog";
            this.btn_fileDialog.Size = new System.Drawing.Size(117, 36);
            this.btn_fileDialog.TabIndex = 0;
            this.btn_fileDialog.Text = "Выбрать файл";
            this.btn_fileDialog.UseVisualStyleBackColor = true;
            this.btn_fileDialog.Click += new System.EventHandler(this.btn_fileDialog_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(328, 161);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 36);
            this.button1.TabIndex = 1;
            this.button1.Text = "Проверка";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_Load
            // 
            this.btn_Load.Location = new System.Drawing.Point(527, 161);
            this.btn_Load.Name = "btn_Load";
            this.btn_Load.Size = new System.Drawing.Size(121, 35);
            this.btn_Load.TabIndex = 2;
            this.btn_Load.Text = "Прошить";
            this.btn_Load.UseVisualStyleBackColor = true;
            this.btn_Load.Click += new System.EventHandler(this.btn_Load_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_Load);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_fileDialog);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_fileDialog;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_Load;
    }
}

