
namespace VisionHandwritingICR
{
    partial class CropImageDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CropImageDialog));
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.CurrentPicture = new System.Windows.Forms.PictureBox();
            this.LocationDisplay = new System.Windows.Forms.Label();
            this.PictureSize = new System.Windows.Forms.Label();
            this.PicContainer = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentPicture)).BeginInit();
            this.PicContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ConfirmButton.Location = new System.Drawing.Point(0, 526);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(584, 35);
            this.ConfirmButton.TabIndex = 1;
            this.ConfirmButton.Text = "Hoàn tất";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CurrentPicture
            // 
            this.CurrentPicture.Image = ((System.Drawing.Image)(resources.GetObject("CurrentPicture.Image")));
            this.CurrentPicture.Location = new System.Drawing.Point(92, 0);
            this.CurrentPicture.Name = "CurrentPicture";
            this.CurrentPicture.Size = new System.Drawing.Size(402, 526);
            this.CurrentPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CurrentPicture.TabIndex = 2;
            this.CurrentPicture.TabStop = false;
            this.CurrentPicture.Click += new System.EventHandler(this.CurrentPicture_Click);
            this.CurrentPicture.Paint += new System.Windows.Forms.PaintEventHandler(this.CurrentPicture_Paint);
            this.CurrentPicture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CurrentPicture_MouseDown);
            this.CurrentPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CurrentPicture_MouseMove);
            this.CurrentPicture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CurrentPicture_MouseUp);
            // 
            // LocationDisplay
            // 
            this.LocationDisplay.AutoSize = true;
            this.LocationDisplay.BackColor = System.Drawing.SystemColors.Control;
            this.LocationDisplay.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LocationDisplay.Location = new System.Drawing.Point(0, 511);
            this.LocationDisplay.Name = "LocationDisplay";
            this.LocationDisplay.Size = new System.Drawing.Size(43, 15);
            this.LocationDisplay.TabIndex = 3;
            this.LocationDisplay.Text = "Tọa độ";
            this.LocationDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PictureSize
            // 
            this.PictureSize.AutoSize = true;
            this.PictureSize.BackColor = System.Drawing.SystemColors.Control;
            this.PictureSize.Dock = System.Windows.Forms.DockStyle.Left;
            this.PictureSize.Location = new System.Drawing.Point(0, 0);
            this.PictureSize.Name = "PictureSize";
            this.PictureSize.Size = new System.Drawing.Size(65, 15);
            this.PictureSize.TabIndex = 4;
            this.PictureSize.Text = "Kích cỡ pic";
            this.PictureSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PicContainer
            // 
            this.PicContainer.Controls.Add(this.CurrentPicture);
            this.PicContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicContainer.Location = new System.Drawing.Point(0, 0);
            this.PicContainer.Name = "PicContainer";
            this.PicContainer.Size = new System.Drawing.Size(584, 526);
            this.PicContainer.TabIndex = 5;
            // 
            // CropImageDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.ControlBox = false;
            this.Controls.Add(this.PictureSize);
            this.Controls.Add(this.LocationDisplay);
            this.Controls.Add(this.PicContainer);
            this.Controls.Add(this.ConfirmButton);
            this.DoubleBuffered = true;
            this.Name = "CropImageDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cắt xém hình ảnh";
            ((System.ComponentModel.ISupportInitialize)(this.CurrentPicture)).EndInit();
            this.PicContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.PictureBox CurrentPicture;
        private System.Windows.Forms.Label LocationDisplay;
        private System.Windows.Forms.Label PictureSize;
        private System.Windows.Forms.Panel PicContainer;
    }
}