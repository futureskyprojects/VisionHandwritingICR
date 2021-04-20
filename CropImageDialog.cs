using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VisionHandwritingICR
{
    public partial class CropImageDialog : Form
    {
        public Bitmap ProcessedBitmap { get; set; }

        private Image<Bgr, byte> CurrentImage { get; set; }

        private float WRatio = 0F;
        private float HRatio = 0F;

        private Rectangle _Rectangle;

        private Rectangle BigRectagle;

        Point StartLocation;
        Point EndLocation;
        bool IsMouseDown = false;

        public CropImageDialog(string imagePath)
        {
            var bitmap = new Bitmap(imagePath);
            ProcessedBitmap = bitmap;
            CurrentImage = new Image<Bgr, byte>(imagePath);

            InitializeComponent();

            InitFormSize();
            InitPicture();
        }

        private void InitFormSize()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            float ratio = 0F;
            var standardSize = 0;
            if (ProcessedBitmap.Width > ProcessedBitmap.Height)
            {
                //standardSize = (int)(Screen.PrimaryScreen.Bounds.Width * 0.8);
                standardSize = PicContainer.Width;
                ratio = (float)ProcessedBitmap.Height / ProcessedBitmap.Width;
                CurrentPicture.Size = new Size(standardSize, (int)(standardSize * ratio));
            }
            else
            {
                //standardSize = (int)(Screen.PrimaryScreen.Bounds.Height * 0.8);
                standardSize = PicContainer.Height;
                ratio = (float)ProcessedBitmap.Width / ProcessedBitmap.Height;
                CurrentPicture.Size = new Size((int)(standardSize * ratio), standardSize);

                WRatio = (float)ProcessedBitmap.Width / (standardSize * ratio);
                HRatio = (float)ProcessedBitmap.Height / standardSize;
            }

            var startX = (PicContainer.Width - CurrentPicture.Width) / 2;
            var startY = (PicContainer.Height - CurrentPicture.Height) / 2;

            CurrentPicture.Location = new Point(startX, startY);

            //Padding p = new System.Windows.Forms.Padding();
            //p.Left = startX;
            //p.Top = startY;
            //CurrentPicture.Padding = p;

            PictureSize.Text = $"W: {CurrentPicture.Size.Width} - H: {CurrentPicture.Size.Height}";
        }

        private void InitPicture()
        {
            CurrentPicture.Image = ProcessedBitmap;

            if (CurrentPicture.Width > CurrentPicture.Height)
            {

            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (_Rectangle != null)
            {
                var temp = new Rectangle();
                temp.X = (int)(_Rectangle.X * WRatio);
                temp.Y = (int)(_Rectangle.Y * HRatio);
                temp.Width = (int)(_Rectangle.Width * WRatio);
                temp.Height = (int)(_Rectangle.Height * HRatio);

                CurrentImage.ROI = temp;
                ProcessedBitmap = CurrentImage.ToBitmap();
                CurrentPicture.Image = ProcessedBitmap;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CurrentPicture_MouseDown(object sender, MouseEventArgs e)
        {
            IsMouseDown = true;
            StartLocation = e.Location;
        }

        private void CurrentPicture_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown == true)
            {
                EndLocation = e.Location;
                CurrentPicture.Invalidate();
            }
        }

        private void CurrentPicture_Paint(object sender, PaintEventArgs e)
        {
            if (_Rectangle != null)
            {
                e.Graphics.DrawRectangle(Pens.Red, GetRectangle());
            }
        }

        private Rectangle GetRectangle()
        {
            _Rectangle = new Rectangle();
            _Rectangle.X = Math.Min(StartLocation.X, EndLocation.X);
            _Rectangle.Y = Math.Min(StartLocation.Y, EndLocation.Y);
            _Rectangle.Width = Math.Abs(StartLocation.X - EndLocation.X);
            _Rectangle.Height = Math.Abs(StartLocation.Y - EndLocation.Y);

            LocationDisplay.Text = $"sXY: ({StartLocation.X},{StartLocation.Y}) - sY: ({EndLocation.X},{EndLocation.Y})";
            return _Rectangle;
        }

        private void CurrentPicture_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseDown == true)
            {
                EndLocation = e.Location;
                IsMouseDown = false;
            }
        }

        private void CurrentPicture_Click(object sender, EventArgs e)
        {

        }
    }
}
