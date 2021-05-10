using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionHandwritingICR.Processing;

namespace VisionHandwritingICR
{
    public partial class MainForm : Form
    {

        private string CurrentImagePath = "";

        private Image<Bgr, byte>[][] CurrentContentAreas;

        public MainForm()
        {
            InitializeComponent();
            InitSomeAttributes();
            if (!File.Exists(RuntimeController.GetAPIKeyFilePath()))
            {
                APIAuthorizeFail();
            }
            else
            {
                APIAuthorizeOK();
            }
            InitSheetParams();
        }


        private void InitSomeAttributes()
        {
        }

        private void InitSheetParams()
        {
            NameOfNewExcel.Text = Path.GetFileNameWithoutExtension(RuntimeController.GetExcelExportNewFileName(NameOfNewExcel.Text));
            NameOfFirstSheet.Text = "Sheet1";
        }

        private string ExportExcelProcessing()
        {
            var currentFileInfo = new FileInfo(RuntimeController.GetExcelExportNewFileName(NameOfNewExcel.Text));

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
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

            if (!File.Exists(RuntimeController.GetAPIKeyFilePath()))
            {
                MessageBox.Show("Vui lòng cung cấp tệp *.json chứng thực để sử dụng phần mềm", "CHƯA CHỨNG THỰC",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (NumberOfColumn.Value <= 0)
            {
                MessageBox.Show("Só cột quét cần lớn hơn 0", "DỮ LIỆU KHÔNG HỢP LỆ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessingData();

        }

        private void ProcessingData()
        {
            var client = ImageAnnotatorClient.Create();

            var loadingDialog = new LoadingForm();
            new Thread(async () =>
            {
                var scanColumnCount = NumberOfColumn.Value;
                DataTable dt = new DataTable();

                var cols = await RuntimeController.GetHeaderColumns();
                // Khởi tạo danh sách cột
                foreach (var col in cols)
                {
                    dt.Columns.Add(col);
                }


                var orc = new Tesseract(RuntimeController.GetTesseractViModelDirectory(), "vie", OcrEngineMode.TesseractLstmCombined);
                var number = new Tesseract(RuntimeController.GetTesseractViModelDirectory(), "vie", OcrEngineMode.TesseractOnly, "1234567890.,");
                for (int i = 1; i < CurrentContentAreas.Length; i++)
                {
                    var dr = dt.NewRow();
                    for (int j = 0; j < CurrentContentAreas[i].Length; j++)
                    {
                        var currentImg = CurrentContentAreas[i][j];

                        // Xử lý bao countours

                        if (j == 0)
                        {
                            dr[j] = i.ToString();
                            continue;
                        }
                        else if (j > 2 && j <= 2 + scanColumnCount)
                        {
                            // sử dụng tesseract
                            //number.SetImage(currentImg);
                            //number.Recognize();

                            //var res = number.GetUTF8Text()
                            //    .Trim();
                            //dr[j] = res;
                            // sử dụng gg API
                            try
                            {
                                currentImg.ToBitmap().Save("./temp.jpg", ImageFormat.Jpeg);

                                var image = await Google.Cloud.Vision.V1.Image.FromFileAsync("./temp.jpg");
                                var response = client.DetectText(image);

                                var resultCollection = new List<string>();
                                foreach (var annotation in response)
                                {
                                    if (annotation.Description != null)
                                    {
                                        annotation.Description = Regex.Replace(annotation.Description,
                                            @"[^0-9.,]", "", RegexOptions.Multiline)
                                        .Trim()
                                        .Trim('.')
                                        .Trim(',');
                                        resultCollection.Add(annotation.Description);
                                    }
                                }
                                dr[j] = GetBestScores(resultCollection);
                            }
                            catch (Exception e)
                            {
                                
                            }
                        }
                        else if (j == 1 || j == 2)
                        {
                            // sử dụng tesseract
                            orc.SetImage(currentImg);
                            orc.Recognize();

                            var res = orc.GetUTF8Text()
                                .Trim();

                            // Xử lý dành cho mã học viên
                            if (j == 1)
                            {
                                dr[j] = ProcessStudentCode(res);
                            }
                            else if (j == 2)
                            {
                                dr[j] = ProcessStudentName(res);
                            }
                            else
                            {
                                dr[j] = res;
                            }
                        }
                    }
                    dt.Rows.Add(dr);
                }
                Invoke(new Action(() =>
                {
                    ResultData.DataSource = dt;
                    loadingDialog.Close();
                    InitSheetParams();

                    MessageBox.Show("Đã nhận diện xong", "THÀNH CÔNG", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }).Start();
            loadingDialog.ShowDialog();
        }

        private string GetBestScores(List<string> resultCollection)
        {
            resultCollection = resultCollection
                .Where(x => Regex.IsMatch(x.Trim(), @"^(\d[.,]\d)|\d$"))
                .Select(x => x.Trim().Replace(",", "."))
                .ToList();
            return resultCollection.FirstOrDefault<string>() ?? "";
        }

        private object ProcessStudentName(string res)
        {
            return Regex
                .Replace(res, @"[^a-z0-9A-Z ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ]", "", RegexOptions.Multiline)
                .Replace("|", "")
                .Trim();
        }

        private object ProcessStudentCode(string res)
        {
            Match m = Regex.Match(res, @"\w{5,}", RegexOptions.CultureInvariant);
            return m.Value;
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
            }
        }

        private void StartProcessCroppedImage(Bitmap bmp)
        {
            var loadingDialog = new LoadingForm();
            new Thread(() =>
            {
                Image<Bgr, byte> croppedImage = null;
                try
                {
                    croppedImage = Pre.Processing((Bitmap)bmp.Clone());
                }
                catch (Exception)
                {
                    Invoke(new Action(() =>
                    {
                        loadingDialog.Close();
                        MessageBox.Show("Không thể căn chỉnh ảnh, vui lòng sử dụng công cụ để cắt lại hoặc chọn một ảnh khác tuân thủ theo hướng dẫn",
                            "XỬ LÝ KHÔNG ĐƯỢC",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }));
                    return;
                }
                CurrentContentAreas = ExtractDataAreas.Processing(croppedImage);
                Invoke(new Action(() =>
                {
                    CurrentPhoto.Image = croppedImage.ToBitmap();
                    loadingDialog.Close();
                }));
            }).Start();
            loadingDialog.ShowDialog();
        }



        private void OpenProcessCropImage()
        {
            using var cropImageForm = new CropImageDialog(CurrentImagePath);
            var cropImageDialogResult = cropImageForm.ShowDialog();
            if (cropImageDialogResult == DialogResult.OK)
            {
                var croppedImage = cropImageForm.ProcessedBitmap;
                CurrentPhoto.Image = croppedImage;
                StartProcessCroppedImage(croppedImage);
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
                File.Delete(RuntimeController.GetAPIKeyFilePath());
                File.Copy(dlg.FileName, RuntimeController.GetAPIKeyFilePath());
                APIAuthorizeOK();
            }
        }

        private void APIAuthorizeOK()
        {
            APIAuthorizePath.Text = "Nhấp để thay đổi";
            APIAuthorizePath.ReadOnly = true;
            APIAuthorizePath.BackColor = Color.Green;

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", RuntimeController.GetAPIKeyFilePath());
        }

        private void APIAuthorizeFail()
        {
            APIAuthorizePath.Text = "Nhấp để thêm";
            APIAuthorizePath.ReadOnly = true;
            APIAuthorizePath.BackColor = Color.Red;
        }


        private void ExportExcel_Click(object sender, EventArgs e)
        {
            if (ResultData.RowCount <= 0 && ResultData.ColumnCount <= 0)
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

            if (File.Exists(RuntimeController.GetExcelExportNewFileName(NameOfNewExcel.Text)))
            {
                MessageBox.Show("Vui lòng đặt tên khác cho tệp excel xuất", "ĐÃ TỒN TẠI",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var resPath = ExportExcelProcessing();
            var dlgRes = MessageBox.Show($"Đã xuất xong tại đường dẫn [{resPath}], mở thư mục chứa kết quả?", "THÀNH CÔNG",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            InitSheetParams();
            if (dlgRes == DialogResult.Yes)
            {
                var folder = Path.GetDirectoryName(resPath);
                //Process.Start(folder);
                Process.Start(new ProcessStartInfo()
                {
                    FileName = folder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private void ResultData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
