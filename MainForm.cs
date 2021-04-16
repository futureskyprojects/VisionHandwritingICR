using Google.Cloud.Vision.V1;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
        }

        private void Processing()
        {
            ResultDetectedStrings.Clear();
            //ResultDetectedStrings = new List<string> {
            //"Cột 1",
            //"Cột 2",
            //"Cột 3",
            //"Data 1",
            //"Data 2",
            //"Data 3"
            //};
            //return;
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
            var currentAppPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            currentAppPath = Path.Combine(currentAppPath, NameOfNewExcel.Text, ".xlsx");
            var currentFileInfo = new FileInfo(currentAppPath);

            using ExcelPackage pck = new ExcelPackage(currentFileInfo);
            BindingSource bs = (BindingSource)ResultData.DataSource;
            DataTable table = (DataTable)bs.DataSource;
            DataTable filtered = table.DefaultView.ToTable();

            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(NameOfFirstSheet.Text);
            ws.Cells["A1"].LoadFromDataTable(filtered, true, OfficeOpenXml.Table.TableStyles.Light1);
            //using (ExcelRange rng = ws.Cells[1, 1, 1, ResultData.Columns.Count])
            //{
            //    rng.Style.Font.Bold = true;
            //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
            //    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
            //}

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
                CurrentPhoto.Image = new Bitmap(dlg.FileName);
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
            try
            {
                var resPath = ExportExcelProcessing();
                var dlgRes = MessageBox.Show("Đã xuất xong, mở thư mục chứa kết quả?", "THÀNH CÔNG",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dlgRes == DialogResult.Yes)
                {
                    Process.Start(resPath);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Xuất Excel không thành công!", "XUẤT KHÔNG THÀNH CÔNG",
    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
