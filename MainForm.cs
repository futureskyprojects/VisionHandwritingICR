using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Google.Cloud.Vision.V1;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionHandwritingICR
{
    public partial class MainForm : Form
    {
        private List<string> ResultDetectedStrings = new List<string>();

        private string CurrentImagePath = "";

        public MainForm()
        {
            InitializeComponent();
            InitSomeAttributes();
            APIAuthorizePath.Text = @"‪C:\Vistark\securityKey.json";
            InitSheetParams();
            //PREVIEWIMAGE.Hide();
        }

        private void Processing()
        {
            ResultDetectedStrings.Clear();
            ResultDetectedStrings = new List<string> {
            "Cột 1",
            "Cột 2",
            "Cột 3",
            "Data 1",
            "Data 2",
            "Data 3",
            "Data 3",
            "Data 3",
            "Data 3",
            "Data 3",
            "Data 3",
            "Data 3",
            };
            return;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", APIAuthorizePath.Text.ToString());
            var client = ImageAnnotatorClient.Create();
            var image = Google.Cloud.Vision.V1.Image.FromFile(CurrentImagePath);
            var response = client.DetectText(image);
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                {
                    ResultDetectedStrings.Add(annotation.Description);
                }
            }
        }

        private void InitSomeAttributes()
        {
        }

        private void BuildListViewDataFromStringResult()
        {
            DataTable dt = new DataTable();

            var columnCount = (int)NumberOfColumn.Value;
            // Phân giải tên cột
            for (int i = 0; i < columnCount; i++)
                dt.Columns.Add(ResultDetectedStrings[i]);

            // Phân giải nội dung các cột
            var rowCount = (int)Math.Ceiling((((float)ResultDetectedStrings.Count - columnCount) / columnCount));
            for (int i = 0; i < rowCount; i++)
            {
                var dr = dt.NewRow();
                for (int j = 0; j < columnCount; j++)
                {
                    var currentIndex = (i + 1) * columnCount + j;
                    if (currentIndex < ResultDetectedStrings.Count)
                    {
                        string currentValue = ResultDetectedStrings[currentIndex];
                        dr[j] = currentValue;
                    }
                }
                dt.Rows.Add(dr);
            }
            ResultData.DataSource = dt;
        }

        private void InitSheetParams()
        {
            NameOfNewExcel.Text = $"Table_{Guid.NewGuid()}";
            NameOfFirstSheet.Text = "Sheet1";
        }

        private string ExportExcelProcessing()
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "Exports");

            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }

            currentAppPath = Path.Combine(currentAppPath, NameOfNewExcel.Text + ".xlsx");

            var currentFileInfo = new FileInfo(currentAppPath);

            using ExcelPackage pck = new ExcelPackage(currentFileInfo);

            DataTable table = (DataTable)ResultData.DataSource;
            DataTable filtered = table.DefaultView.ToTable();

            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(NameOfFirstSheet.Text);
            ws.Cells["A1"].LoadFromDataTable(filtered, true, OfficeOpenXml.Table.TableStyles.Light1);
            pck.Save();

            return currentFileInfo.FullName;

        }

        private void StartDetectText_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentImagePath))
            {
                MessageBox.Show("Vui lòng chọn ảnh để xử lý", "CHƯA CHỌN ẢNH",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(APIAuthorizePath.Text.ToString()))
            {
                MessageBox.Show("Vui lòng cung cấp tệp *.json chứng thực", "CHƯA CHỨNG THỰC",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ResultData.Rows.Clear();
            ResultData.Columns.Clear();

            Processing();

            BuildListViewDataFromStringResult();

            InitSheetParams();

            MessageBox.Show("Đã nhận diện xong", "THÀNH CÔNG",
MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CurrentPhoto_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Chọn ảnh có chữa dữ liệu viết tay";
            dlg.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CurrentImagePath = dlg.FileName;
                //OpenProcessCropImage();
                ProcessWithCv2(dlg.FileName);
            }
        }

        private void OpenProcessCropImage()
        {
            using var cropImageForm = new CropImageDialog(CurrentImagePath);
            var cropImageDialogResult = cropImageForm.ShowDialog();
            if (cropImageDialogResult == DialogResult.OK)
            {
                var croppedImage = cropImageForm.ProcessedBitmap;
                CurrentPhoto.Image = croppedImage;
            }
        }

        private void APIAuthorizePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void APIAuthorizePath_MouseClick(object sender, MouseEventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Chọn tệp chứng thực";
            dlg.Filter = "Json Files (*.json)|*.JSON";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                APIAuthorizePath.Text = dlg.FileName;
            }
        }

        private void ExportExcel_Click(object sender, EventArgs e)
        {
            if (ResultDetectedStrings == null || ResultDetectedStrings.Count <= 0)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất", "CHƯA CÓ DỮ LIỆU",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(NameOfNewExcel.Text))
            {
                MessageBox.Show("Vui lòng đặt tên cho tệp Excel mới", "CHƯA ĐẶT TÊN",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(NameOfFirstSheet.Text))
            {
                MessageBox.Show("Vui lòng đặt tên cho trang tính đầu tiên", "CHƯA ĐẶT TÊN",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //        try
            //        {
            //            var resPath = ExportExcelProcessing();
            //            var dlgRes = MessageBox.Show("Đã xuất xong, mở thư mục chứa kết quả?", "THÀNH CÔNG",
            //                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //            if (dlgRes == DialogResult.Yes)
            //            {
            //                Process.Start(resPath);
            //            }
            //        }
            //        catch (Exception)
            //        {
            //            MessageBox.Show("Xuất Excel không thành công!", "XUẤT KHÔNG THÀNH CÔNG",
            //MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }
            var resPath = ExportExcelProcessing();
            var dlgRes = MessageBox.Show($"Đã xuất xong tại đường dẫn [{resPath}], mở thư mục chứa kết quả?", "THÀNH CÔNG",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dlgRes == DialogResult.Yes)
            {
                Process.Start(resPath);
            }
        }


        /// <summary>
        /// Bị gãy line khi detect
        /// </summary>
        /// <param name="imagePath"></param>
        private void ProcessGetLineInImage(string imagePath)
        {
            var imgInput = new Image<Bgr, byte>(imagePath);
            Image<Bgr, byte> res = imgInput.Copy();

            var crrVar = res.Convert<Gray, byte>()
                .ThresholdBinary(new Gray(125), new Gray(255));
            LineSegment2D[] lines =
                crrVar
                .Canny(100, 100)
                .HoughLinesBinary(1, Math.PI / 16, 1, 10, 1)[0];
            foreach (LineSegment2D line in lines)
            {
                res.Draw(line, new Bgr(Color.Red), 2);
            }
            PREVIEWIMAGE.Image = res.ToBitmap();
        }


        private void ProcessWithCv2(string imgPath)
        {
            var inputImg = Cv2.ImRead(imgPath);
            var binaryImg = inputImg.EmptyClone();
            Cv2.Threshold(inputImg, binaryImg, 125, 255, ThresholdTypes.Binary);

            var bmp1 = ByteToBitmap(binaryImg.ToBytes());
            CurrentPhoto.Image = bmp1;
            bmp1.Save(@"C:\Users\servi\Downloads\file_img_bina.jpg", ImageFormat.Jpeg);


            var rawBina = new Image<Bgr, byte>(@"C:\Users\servi\Downloads\file_img_bina.jpg");
            var processImg = rawBina.Convert<Gray, byte>();
            // Lấy contour
            Emgu.CV.Mat hierarchy = new Emgu.CV.Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(processImg, contours, hierarchy, RetrType.List,
                ChainApproxMethod.ChainApproxNone);

            // Step 3 : contour fusion
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            for (int i = 0; i < contours.Size; i++)
            {
                if (contours[i].ToArray().Any(x => x.X == 0 || x.Y == 0))
                    continue;

                points.AddRange(contours[i].ToArray());

                var rect = CvInvoke.BoundingRectangle(contours[i]);
                rawBina.Draw(rect, new Bgr(0, 255, 0), 1);

                //CvInvoke.FillPoly(processImg, contours[i], new MCvScalar(0, 0, 255));
                //rawBina.Draw(contours[i].ToArray(), new Bgr(0, 255, 0), 2);
                //processImg.DrawPolyline(contours[i].ToArray(), true, new Bgr(0, 255, 0), 3);
                //CvInvoke.Draw(processImg, contours[i], 0, new MCvScalar(255, 0, 0));
            }



            PREVIEWIMAGE.Image = rawBina.ToBitmap();
            rawBina.ToBitmap().Save(@"C:\Users\servi\Downloads\file_img_prv.jpg", ImageFormat.Jpeg);
        }


        private Bitmap ByteToBitmap(byte[] content)
        {
            Bitmap bmp;
            using var ms = new MemoryStream(content);
            bmp = new Bitmap(ms);
            return bmp;
        }

        /// <summary>
        /// Vẫn không được
        /// </summary>
        /// <param name="imagePath"></param>
        //private void ProcessRectangleInImage(string imagePath)
        //{
        //    var imgInput = new Image<Bgr, byte>(imagePath);
        //    Image<Bgr, byte> res = imgInput.Copy();

        //    var crrVar = res.Convert<Gray, byte>()
        //        .ThresholdBinary(new Gray(100), new Gray(255));

        //    CurrentPhoto.Image = crrVar.ToBitmap();
        //    crrVar.ToBitmap().Save(@"C:\Users\servi\Downloads\file_img_bina.jpg", ImageFormat.Jpeg);

        //    // Lấy contour 
        //    Mat hierarchy = new Mat();
        //    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        //    CvInvoke.FindContours(crrVar, contours, hierarchy, RetrType.List,
        //        ChainApproxMethod.ChainApproxNone);

        //    //Step 3 : contour fusion
        //    List<Point> points = new List<Point>();
        //    for (int i = 0; i < contours.Size; i++)
        //    {
        //        if (contours[i].ToArray().Any(x => x.X == 0 || x.Y == 0))
        //            continue;

        //        points.AddRange(contours[i].ToArray());

        //        //var rect = CvInvoke.BoundingRectangle(contours[i]);
        //        //imgInput.Draw(rect, new Bgr(0, 255, 0), 1);

        //        //CvInvoke.FillPoly(imgInput, contours[i], new MCvScalar(0, 0, 255));
        //        imgInput.Draw(contours[i].ToArray(), new Bgr(0, 255, 0), 2);
        //        //imgInput.DrawPolyline(contours[i].ToArray(), true, new Bgr(0, 255, 0), 3);
        //        //CvInvoke.Draw(imgInput, contours[i], 0, new MCvScalar(255, 0, 0));
        //    }

        //    //Step 4 : Rotated rect
        //    //RotatedRect minAreaRect = CvInvoke.MinAreaRect(points.Select(pt => new PointF(pt.X, pt.Y)).ToArray());
        //    //Point[] vertices = minAreaRect.GetVertices().Select(pt => new Point((int)pt.X, (int)pt.Y)).ToArray();
        //    ////Step 5 : draw result
        //    //imgInput.Draw(vertices, new Bgr(Color.Red), 2);

        //    PREVIEWIMAGE.Image = imgInput.ToBitmap();
        //    imgInput.ToBitmap().Save(@"C:\Users\servi\Downloads\file_img.jpg", ImageFormat.Jpeg);

        //    //CurrentPhoto.Image = crrVar.ToBitmap();
        //    //// Math.PI / 16
        //    //LineSegment2D[] lines =
        //    //    crrVar
        //    //    .HoughLinesBinary(0.02, Math.PI / 300, 10, 20, 2)[0];
        //    //foreach (LineSegment2D line in lines)
        //    //{
        //    //    res.Draw(line, new Bgr(Color.Red), 2);
        //    //}
        //    //PREVIEWIMAGE.Image = res.ToBitmap();
        //}

        private void ResultData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
