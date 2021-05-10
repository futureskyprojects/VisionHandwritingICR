using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace VisionHandwritingICR.Processing
{
    public class Pre
    {
        // Có bật chế độ debug bước cắt 1 hay không
        public bool IsDebug { get; set; } = true;

        // Ảnh gốc đã load
        public Image<Bgr, byte> RawImage { get; set; }


        public static Image<Bgr, byte> Processing(string imagePath)
        {
            var preProcess = new Pre();
            var step1BinaryImage = preProcess.ConvertToBinary(imagePath);
            var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);
            var step3BigestContours = preProcess.GetBigestContours(step2BinaryInvertImage);
            return preProcess.PerspectiveTransform(step3BigestContours);
            //return ReProcessing(step4);

        }

        public static Image<Bgr, byte> Processing(Bitmap bmp)
        {
            var preProcess = new Pre();
            preProcess.RawImage = bmp.ToImage<Bgr, byte>();
            var step1BinaryImage = preProcess.ConvertToBinary(preProcess.RawImage);
            var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);
            var step3BigestContours = preProcess.GetBigestContours(step2BinaryInvertImage);
            return preProcess.PerspectiveTransform(step3BigestContours);
            //return ReProcessing(step4);

        }

        public static Image<Bgr, byte> ReProcessing(Image<Bgr, byte> inp)
        {
            var res = inp;
            while (true)
            {
                try
                {
                    var preProcess = new Pre();
                    preProcess.IsDebug = false;
                    preProcess.RawImage = inp;
                    var step1BinaryImage = preProcess.ConvertToBinary(inp);
                    var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);
                    if (step2BinaryInvertImage == null)
                        break;
                    var step3BigestContours = preProcess.GetBigestContours(step2BinaryInvertImage);

                    if (step3BigestContours != null)
                    {
                        res = preProcess.PerspectiveTransform(step3BigestContours);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Bước 1: Load ảnh và chuyển thành nhị phân
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public Image<Gray, byte> ConvertToBinary(string imagePath)
        {
            // Đọc ảnh vào
            RawImage = new Image<Bgr, byte>(imagePath);
            return ConvertToBinary(RawImage);
        }

        /// <summary>
        /// Bước 1: Load ảnh và chuyển thành nhị phân
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public Image<Gray, byte> ConvertToBinary(Image<Bgr, byte> inp)
        {
            // Chuyển ảnh thành ảnh xám
            var imgGray = inp.Convert<Gray, byte>();
            // Smooth image
            imgGray = imgGray.SmoothGaussian(3);
            // Chuyển thành ảnh nhị phân
            var imgBinarize = new Image<Gray, byte>(imgGray.Width, imgGray.Height, new Gray(0));
            // - Phân ngưỡng nhị phân
            CvInvoke.Threshold(imgGray, imgBinarize, 128, 255, ThresholdType.Binary | ThresholdType.Otsu);
            // - Phân ngưỡng otsu
            //CvInvoke.Threshold(imgGray, imgBinarize, 50, 255, );
            if (IsDebug)
            {
                // Lưu kết quả debug
                imgBinarize.ToBitmap().CurrentSave("__#1_binary.jpg");
            }
            return imgBinarize;
        }

        /// <summary>
        /// Bước 2: Chuyển thành ảnh nhị phân đảo ngược
        /// </summary>
        /// <param name="binary"></param>
        /// <returns></returns>
        public Image<Gray, byte> ConvertToInvertBinary(Image<Gray, byte> binary)
        {
            var invertImage = binary.Not();
            if (IsDebug)
            {
                invertImage.ToBitmap().CurrentSave("__#2_binary_invert.jpg");
            }
            return invertImage;
        }

        /// <summary>
        /// Bước 4: Lấy phần viền bao lớn nhất
        /// </summary>
        /// <param name="invertBinary"></param>
        /// <returns></returns>
        public Image<Gray, byte> GetBigestContours(Image<Gray, byte> invertBinary)
        {
            var rawImageClone = RawImage.Clone();
            // Lấy danh sách viền bao
            VectorOfVectorOfPoint contours = FindAllContours(invertBinary);

            // Biến chứa viền bao lớn nhất
            double largestArea = 0;
            IInputArray biggestContourPoins = null;

            // Biến chứa diện tích lớn nhì
            double sendcondLargestArea = 0;
            IInputArray sendcondLargestPoint = null;

            // Tìm kiếm trong danh sách các viền bao
            for (int i = 0; i < contours.Size; i++)
            {
                if (contours[i].ToArray().Any(x => x.X == 0 || x.Y == 0))
                    continue;

                var rect = CvInvoke.BoundingRectangle(contours[i]);

                if (IsDebug)
                {
                    // Vẽ viền bao vào ảnh gốc
                    rawImageClone.Draw(rect, new Bgr(0, 255, 0), 1);
                }

                // Đếm diện tích trong của viền
                var contourCountArea = CvInvoke.ContourArea(contours[i]);
                // Nếu diện tích trong lớn hơn diện tích lớn nhất đã có
                if (contourCountArea > largestArea)
                {
                    // Lưu mới thay thế
                    largestArea = contourCountArea;
                    biggestContourPoins = contours[i];
                }

                if (contourCountArea > sendcondLargestArea && contourCountArea < largestArea)
                {
                    sendcondLargestArea = contourCountArea;
                    sendcondLargestPoint = contours[i];
                }
            }

            // Nếu không có bất cứ viền nào lớn hơn 60% diện tích ảnh thì bỏ qua
            // Hoặc 4 điểm chạm viền thì bỏ qua
            try
            {
                var rectaz = CvInvoke.BoundingRectangle(biggestContourPoins);
                if (biggestContourPoins == null || rectaz == null)
                {
                    return null;
                }
                if ((rectaz.Width < invertBinary.Width * 0.6 ||
                rectaz.Contains(new Point(0, 0))) &&
                rectaz.Width < invertBinary.Width * 0.6)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (IsDebug)
            {
                // Vẽ đậm lại viền lớn nhất
                var rect = CvInvoke.BoundingRectangle(biggestContourPoins);

                if (IsDebug)
                {
                    // Vẽ viền bao vào ảnh gốc
                    rawImageClone.Draw(rect, new Bgr(255, 0, 0), 5);
                }
                // Lưu ảnh viền bao đầy đủ
                rawImageClone.ToBitmap().CurrentSave("__#3_full_contours.jpg");
            }

            // Lấy khung viền lớn nhất
            var bigestRect = CvInvoke.BoundingRectangle(biggestContourPoins);
            invertBinary.ROI = bigestRect;

            var croppedImage = invertBinary.Copy();

            // Nếu có tồn tại một khung viền nào đó có kích thước lớn hơn 60% của ảnh hiện tại => Cắt nó
            if (sendcondLargestArea > largestArea * 0.6)
            {
                return GetBigestContours(croppedImage);
            }

            if (IsDebug)
            {
                // Lưu ảnh của khung viền đầy đủ
                croppedImage.ToBitmap().CurrentSave("__#4_bigest_contours_in_binaInv.jpg");
                // Lưu ảnh có màu đã được cắt
                RawImage.ROI = bigestRect;
                RawImage = RawImage.Copy();
                RawImage.ToBitmap().CurrentSave("__#5_bigest_contours.jpg");
            }

            return croppedImage;
        }

        /// <summary>
        /// Bước 4: Xoay ảnh cân đối
        /// </summary>
        /// <param name="binaryInvertImage"></param>
        /// <returns></returns>
        public Image<Bgr, byte> PerspectiveTransform(Image<Gray, byte> binaryInvertImage)
        {
            // Lọc phong sương, làm mịn phi tuyến tính, bảo toàn cạnh và giảm nhiễu cho hình ảnh
            var bfImg = binaryInvertImage.CopyBlank();
            CvInvoke.BilateralFilter(binaryInvertImage, bfImg, 1, 60, 100);
            if (IsDebug)
            {
                // Lưu ảnh của lọc nhiễu phong sương
                bfImg.ToBitmap().CurrentSave("__#6_BilateralFilter.jpg");
            }

            #region For vertical line
            var verKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, 50), new Point(-1, -1));
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(-1, -1));

            var verticalLine = binaryInvertImage.CopyBlank();
            CvInvoke.Erode(binaryInvertImage, verticalLine, verKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 0, 0));
            CvInvoke.Dilate(verticalLine, verticalLine, verKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 0, 0));

            if (IsDebug)
            {
                // Lưu kết quả debug
                verticalLine.ToBitmap().CurrentSave("__#7_Vertical_line.jpg");
            }
            #endregion

            #region For horizontal line
            var horKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(50, 1), new Point(-1, -1));

            var horizontalLine = binaryInvertImage.CopyBlank();
            CvInvoke.Erode(binaryInvertImage, horizontalLine, horKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 0, 0));
            CvInvoke.Dilate(horizontalLine, horizontalLine, horKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 0, 0));

            if (IsDebug)
            {
                // Lưu kết quả debug
                horizontalLine.ToBitmap().CurrentSave("__#8_Horizontai_line.jpg");
            }
            #endregion

            #region Combine horizontal and vertical
            var combinedLines = binaryInvertImage.CopyBlank();
            CvInvoke.AddWeighted(verticalLine, 0.5, horizontalLine, 0.5, 0.0, combinedLines);
            if (IsDebug)
            {
                // Lưu kết quả debug
                combinedLines.ToBitmap().CurrentSave("__#9_Combine_line.jpg");
            }
            #endregion

            var combinedLineBitwise = combinedLines.CopyBlank();
            CvInvoke.Erode(combinedLines.Not(), combinedLineBitwise, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(200, 0, 0));
            if (IsDebug)
            {
                // Lưu kết quả debug
                combinedLines.ToBitmap().CurrentSave("__#10_Combine_line_bitwise.jpg");
            }

            var threshold = combinedLineBitwise.CopyBlank();
            CvInvoke.Threshold(combinedLineBitwise, threshold, 60, 255, ThresholdType.Binary | ThresholdType.Otsu);
            if (IsDebug)
            {
                // Lưu kết quả debug
                threshold.ToBitmap().CurrentSave("__#11_Combine_line_bitwise_threshold.jpg");
            }

            // Trích xuất góc
            var corners = new Mat();
            CvInvoke.CornerHarris(threshold, corners, 3);
            CvInvoke.Normalize(corners, corners, 255, 0, NormType.MinMax);

            // Tạo ảnh đem để có thể xem góc xuất hiện
            var blackImage = threshold.CopyBlank().Convert<Bgr, byte>();
            blackImage.SetZero();

            // Chuyển thành ma trận giá trị để lọc
            Matrix<float> matrix = new Matrix<float>(corners.Rows, corners.Cols);
            corners.CopyTo(matrix);

            // Chứa các điểm tạm khớm
            //var matchPoints = new List<Point>();
            Point topLeftCornor = new Point(-1, -1);
            Point topRightCorner = new Point(-1, -1);
            Point bottomLeftCornor = new Point(-1, -1);
            Point bottomRightCornor = new Point(-1, -1);

            var okPoints = new List<Point>();

            // Lặp tìm giá trị góc
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    // && (i == 0 || j == 0 || i == matrix.Rows - 1 || j == matrix.Cols - 1)
                    if (matrix[i, j] > 100)
                    {
                        okPoints.Add(new Point(j, i));

                        if (IsDebug)
                        {
                            // Khoanh tất cả các góc tìm được
                            CvInvoke.Circle(blackImage, new Point(j, i), 5, new MCvScalar(0, 0, 255));
                        }
                    }
                }
            }

            var seprate = 80;
            // Khoanh tất cả các góc tìm được
            CvInvoke.Rectangle(blackImage, new Rectangle(0, 0, seprate, seprate), new MCvScalar(0, 0, 255));

            // Sắp xếp sơ bộ lại trật tự
            topLeftCornor = okPoints
                .Where(x => x.X < seprate && x.Y < seprate)
                .OrderBy(x => x.X)
                .ThenBy(x => x.Y).ToList()
                .First();

            topRightCorner = okPoints
                .Where(x => x.X > matrix.Cols - seprate && x.Y < seprate)
                .OrderBy(x => x.Y)
                .ThenByDescending(x => x.X).ToList()
                .First();

            bottomLeftCornor = okPoints
                .Where(x => x.X < seprate && x.Y > matrix.Rows - seprate)
                .OrderByDescending(x => x.Y)
                .ThenBy(x => x.X).ToList()
                .First();

            bottomRightCornor = okPoints
                .Where(x => x.X > matrix.Cols - seprate && x.Y > matrix.Rows - seprate)
                .OrderByDescending(x => x.Y)
                .ThenByDescending(x => x.X).ToList()
                .First();
            // Phần khớp trên


            if (IsDebug)
            {
                CvInvoke.Circle(blackImage, new Point(0, 0), 10, new MCvScalar(255, 0, 255));
                // Khoanh 4 góc ảnh thực
                CvInvoke.Circle(blackImage, topLeftCornor, 5, new MCvScalar(255, 0, 0), 5);
                CvInvoke.Circle(blackImage, topRightCorner, 5, new MCvScalar(0, 255, 255), 5);
                CvInvoke.Circle(blackImage, bottomLeftCornor, 5, new MCvScalar(255, 255, 0), 5);
                CvInvoke.Circle(blackImage, bottomRightCornor, 5, new MCvScalar(255, 255, 255), 5);
            }



            // Lưu ảnh danh sách góc
            if (IsDebug)
            {
                // Lưu ảnh của trích xuất góc
                blackImage.ToBitmap().CurrentSave("__#12_CornerHarris_all_corners.jpg");
            }

            // Điểm source
            var sources = new PointF[] {
                topLeftCornor,
                topRightCorner,
                bottomLeftCornor,
                bottomRightCornor
            };

            // Tịnh tiến điểm
            PointF[] dsts = new PointF[4];
            dsts[0] = new Point(0, 0);
            dsts[1] = new Point(RawImage.Width, 0);
            dsts[2] = new Point(0, RawImage.Height);
            dsts[3] = new Point(RawImage.Width, RawImage.Height);

            // Tính toán ma trận dịch chuyển
            var homograpryMatrix = CvInvoke.FindHomography(sources, dsts);

            CvInvoke.WarpPerspective(RawImage, RawImage, homograpryMatrix, RawImage.Size);

            // Lưu ảnh danh sách góc
            if (IsDebug)
            {
                // Lưu ảnh của trích xuất góc
                RawImage.ToBitmap().CurrentSave("__#13_WarpPerspective.jpg");
            }
            RawImage.Draw(new Rectangle(new Point(0, 0), RawImage.Size), new Bgr(0, 0, 0), 2);
            return RawImage;
        }



        /// <summary>
        /// Tiến hành tìm tất cả các viền bao có tỏng ảnh
        /// </summary>
        /// <param name="binaryInvert"></param>
        /// <returns></returns>
        public VectorOfVectorOfPoint FindAllContours(Image<Gray, byte> binaryInvert)
        {
            Mat hierarchy = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binaryInvert, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            return contours;
        }

        /// <summary>
        /// Chuyển bytes thành bitmap
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public Bitmap ByteToBitmap(byte[] content)
        {
            Bitmap bmp;
            using var ms = new MemoryStream(content);
            bmp = new Bitmap(ms);
            return bmp;
        }

        /// <summary>
        /// Góc lệch giữa vector và trục hoành
        /// </summary>
        /// <returns></returns>
        public double AngleBetweenVectorAndHorizontalAxis()
        {
            //return AngleBetweenTwoVector()
            return 0.0;
        }

        /// <summary>
        /// Tính góc lệch giữ 2 vector
        /// </summary>
        /// <returns></returns>
        public double AngleBetweenTwoVector(double x1, double y1, double x2, double y2)
        {
            var a = x1 * x2 + y1 * y2;
            var b = Math.Sqrt(x1 * x1 + y1 * y1);
            var c = Math.Sqrt(x2 * x2 + y2 * y2);
            var d = b * c;

            var res = Math.Acos(a / d);
            return res;
        }
    }
}
