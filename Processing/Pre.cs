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
            var step4 = preProcess.PerspectiveTransform(step3BigestContours);
            return ReProcessing(step4);

        }

        public static Image<Bgr, byte> Processing(Bitmap bmp)
        {
            var preProcess = new Pre();
            preProcess.RawImage = bmp.ToImage<Bgr, byte>();
            var step1BinaryImage = preProcess.ConvertToBinary(preProcess.RawImage);
            var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);
            var step3BigestContours = preProcess.GetBigestContours(step2BinaryInvertImage);
            var step4 = preProcess.PerspectiveTransform(step3BigestContours);
            return ReProcessing(step4);

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
            if ((CvInvoke.BoundingRectangle(biggestContourPoins).Width < invertBinary.Width * 0.6 ||
                CvInvoke.BoundingRectangle(biggestContourPoins).Contains(new Point(0, 0))) &&
                CvInvoke.BoundingRectangle(sendcondLargestPoint).Width < invertBinary.Width * 0.6)
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
                rawImageClone.ToBitmap().CurrentSave("__#4_full_contours.jpg");
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
                croppedImage.ToBitmap().CurrentSave("_#4_bigest_contours_in_binaInv.jpg");
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
            CvInvoke.BilateralFilter(binaryInvertImage, bfImg, 5, 75, 75);
            if (IsDebug)
            {
                // Lưu ảnh của lọc nhiễu phong sương
                bfImg.ToBitmap().CurrentSave("__#6_BilateralFilter.jpg");
            }

            // Trích xuất góc
            var corners = new Mat();
            CvInvoke.CornerHarris(bfImg, corners, 2);
            CvInvoke.Normalize(corners, corners, 255, 0, NormType.MinMax);

            // Tạo ảnh đem để có thể xem góc xuất hiện
            var blackImage = bfImg.CopyBlank().Convert<Bgr, byte>();
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

            // Lặp tìm giá trị góc
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    if (matrix[i, j] > 50 && (i == 0 || j == 0 || i == matrix.Rows - 1 || j == matrix.Cols - 1))
                    {
                        // Tìm góc trên bên trái
                        if (j < matrix.Cols / 2 && i < matrix.Rows / 2 && (topLeftCornor.X == -1 || (topLeftCornor.X >= j && topLeftCornor.Y >= i)))
                        {
                            topLeftCornor = new Point(j, i);
                        }
                        // Tìm góc trên bên phải
                        if (j > matrix.Cols / 2 && i < matrix.Rows / 2 && (topRightCorner.X == -1 || (topRightCorner.X <= j && topRightCorner.Y >= i)))
                        {
                            topRightCorner = new Point(j, i);
                        }
                        // Tìm góc dưới bên trái
                        if (j < matrix.Cols / 2 && i > matrix.Rows / 2 && (bottomLeftCornor.X == -1 || (bottomLeftCornor.X >= j && bottomLeftCornor.Y <= i)))
                        {
                            bottomLeftCornor = new Point(j, i);
                        }
                        // Tìm góc dưới phên phải
                        if (j > matrix.Cols / 2 && i > matrix.Rows / 2 && (bottomRightCornor.X == -1 || (bottomRightCornor.X <= j && bottomRightCornor.Y <= i)))
                        {
                            bottomRightCornor = new Point(j, i);
                        }
                        //matchPoints.Add(new Point(j, i));

                        if (IsDebug)
                        {
                            // Khoanh tất cả các góc tìm được
                            CvInvoke.Circle(blackImage, new Point(j, i), 5, new MCvScalar(0, 0, 255));
                        }
                    }
                }
            }

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
                blackImage.ToBitmap().CurrentSave("__#7_CornerHarris_all_corners.jpg");
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
                RawImage.ToBitmap().CurrentSave("__#8_WarpPerspective.jpg");
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
