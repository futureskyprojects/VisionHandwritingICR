using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Google.Cloud.Vision.V1;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VisionHandwritingICR.Processing;

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
        }

        private void Processing()
        {
            ResultDetectedStrings.Clear();
            ResultDetectedStrings = new List<string> {
            "STT",
            "Mã Học viên",
            "Họ tên",
            "TH(20%)",
            "BT/TL(40%)",
            "Đ.Thi(40%)",
            "ĐMH",
            "Số Tờ",
            "1",
            "17ĐH030",
            "Tạ Quốc Vương",
            "7",
            "8",
            "5",
            "6",
            "1",
                ////
            "2",
            "17ĐH029",
            "Nguyễn Văn Vinh",
            "6,5",
            "7,5",
            "2",
            "9",
            "2",
                        ////
            "3",
            "17ĐH028",
            "Phạm Đức Tùng",
            "3",
            "",
            "",
            "",
            "1",
                        ////
            "4",
            "17ĐH027",
            "Nguyễn Minh Tùng",
            "",
            "5",
            "",
            "",
            "1",
                        ////
            "5",
            "17ĐH026",
             "Huỳnh Phạm Trực",
            "",
            "",
            "7",
            "",
            "1",
                        ////
            "6",
            "17ĐH025",
            "Trần Trung Thứ",
            "",
            "",
            "",
            "9",
            "1",
                        ////
            "7",
            "17ĐH024",
            "Lê Sỹ Tấn",
            "",
            "",
            "",
            "",
            "1",
                        ////
            "8",
            "17ĐH023",
            "Lê Hoàng Nhật Tân",
            "",
            "",
            "",
            "",
            "",
                        ////
            "9",
            "17ĐH022",
            "Nguyễn Lâm Minh Nhật",
            "",
            "",
            "",
            "",
            "",
                        ////
            "10",
            "17ĐH021",
            "Phạm Xuân Nguyên",
            "",
            "",
            "",
            "",
            "",
                        ////
            "11",
            "17ĐH020",
            "Lê Văn Nam",
            "",
            "",
            "",
            "",
            "",
                        ////
            "12",
            "17ĐH019",
            "Phan Thanh Lương",
            "",
            "",
            "",
            "",
            "",
                        ////
            "13",
            "17ĐH018",
            "Trần Hữu Lực",
            "",
            "",
            "",
            "",
            "",
                        ////
            "14",
            "17ĐH017",
            "Nguyễn Quang Huy",
            "",
            "",
            "",
            "",
            "",
                        ////
            "15",
            "17ĐH016",
            "Nguyễn Ngọc Huy",
            "",
            "",
            "",
            "",
            "",
                        ////
            "16",
            "17ĐH015",
            "Bùi Kỷ Huy",
            "",
            "",
            "",
            "",
            "",
                        ////
            "17",
            "17ĐH014",
            "Cao Anh Hùng",
            "",
            "",
            "",
            "",
            "",
                         ////
            "18",
            "17ĐH013",
            "Nguyễn Đức Hòa",
            "",
            "",
            "",
            "",
            "",
                         ////
            "19",
            "17ĐH012",
            "Võ Minh Dương",
            "",
            "",
            "",
            "",
            "",
                         ////
            "20",
            "17ĐH011",
            "Nguyễn Đạt Dũng",
            "",
            "",
            "",
            "",
            "",
                         ////
            "21",
            "17ĐH010",
            "Nguyễn Văn Đỉnh",
            "",
            "",
            "",
            "",
            "",
                         ////
            "22",
            "17ĐH009",
            "Hoàng Văn Đạt",
            "",
            "",
            "",
            "",
            "",
                         ////
            "23",
            "17ĐH008",
            "Nguyễn Văn Cường",
            "",
            "",
            "",
            "",
            "",
                         ////
            "24",
            "17ĐH007",
            "Nguyễn Văn Công",
            "",
            "",
            "",
            "",
            "",
                         ////
            "25",
            "17ĐH006",
            "Vương Văn Chính",
            "",
            "",
            "",
            "",
            "",
                         ////
            "26",
            "17ĐH005",
            "Phan Trọng Bình",
            "",
            "",
            "",
            "",
            "",
                         ////
            "27",
            "17ĐH004",
            "Trần Đình Bá",
            "",
            "",
            "",
            "",
            "",

            "28",
            "17ĐH003",
            "Lương Nguyễn Tuấn Anh",
            "",
            "",
            "",
            "",
            "",

            "29",
            "17ĐH002",
            "Hoàng Ngọc Anh",
            "",
            "",
            "",
            "",
            "",

            "30",
            "17ĐH001",
            "Chu Văn An",
            "",
            "",
            "",
            "",
            "",
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
                OpenProcessCropImage();
                var croppedImage = Pre.Processing(CurrentImagePath);
                RemoveText.Processing(croppedImage);
                CurrentPhoto.Image = croppedImage.ToBitmap();
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

        private void ResultData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
