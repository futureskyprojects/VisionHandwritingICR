using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace VisionHandwritingICR.Processing
{
    public class RemoveText
    {
        public bool IsDebug { get; set; } = true;

        // Ảnh gốc đã load
        public Image<Bgr, byte> RawImage { get; set; }

        public int NumberOfColums { get; set; } = 9;
        public static void Processing(Image<Bgr, byte> input, int numberOfColum = 9)
        {
            var removeText = new RemoveText();
            removeText.NumberOfColums = numberOfColum;
            removeText.ExtractKernel(input);
        }

        public Image<Gray, byte> ExtractKernel(Image<Bgr, byte> input)
        {
            RawImage = input;
            var preProcess = new Pre();
            preProcess.IsDebug = false;
            var step1BinaryImage = preProcess.ConvertToBinary(input);
            if (IsDebug)
            {
                // Lưu kết quả debug
                step1BinaryImage.ToBitmap().Save(@"C:\Users\servi\Downloads\__#9_binary.jpg", ImageFormat.Jpeg);
            }
            var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);
            if (IsDebug)
            {
                // Lưu kết quả debug
                step1BinaryImage.ToBitmap().Save(@"C:\Users\servi\Downloads\__#10_binary_inverted.jpg", ImageFormat.Jpeg);
            }

            #region For vertical line
            var verKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, 100), new Point(-1, -1));
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(-1, -1));

            var verticalLine = step2BinaryInvertImage.CopyBlank();
            CvInvoke.Erode(step2BinaryInvertImage, verticalLine, verKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(200, 0, 0));
            CvInvoke.Dilate(verticalLine, verticalLine, verKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(200, 0, 0));

            if (IsDebug)
            {
                // Lưu kết quả debug
                verticalLine.ToBitmap().Save(@"C:\Users\servi\Downloads\__#11_Vertical_line.jpg", ImageFormat.Jpeg);
            }
            #endregion

            #region For horizontal line
            var horKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(100, 1), new Point(-1, -1));

            var horizontalLine = step2BinaryInvertImage.CopyBlank();
            CvInvoke.Erode(step2BinaryInvertImage, horizontalLine, horKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(200, 0, 0));
            CvInvoke.Dilate(horizontalLine, horizontalLine, horKernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(200, 0, 0));

            if (IsDebug)
            {
                // Lưu kết quả debug
                horizontalLine.ToBitmap().Save(@"C:\Users\servi\Downloads\__#12_Horizontai_line.jpg", ImageFormat.Jpeg);
            }
            #endregion

            #region Combine horizontal and vertical
            var combinedLines = step2BinaryInvertImage.CopyBlank();
            CvInvoke.AddWeighted(verticalLine, 0.5, horizontalLine, 0.5, 0.0, combinedLines);
            if (IsDebug)
            {
                // Lưu kết quả debug
                combinedLines.ToBitmap().Save(@"C:\Users\servi\Downloads\__#13_Combine_line.jpg", ImageFormat.Jpeg);
            }
            #endregion

            var combinedLineBitwise = combinedLines.CopyBlank();
            CvInvoke.Erode(combinedLines.Not(), combinedLineBitwise, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(200, 0, 0));
            if (IsDebug)
            {
                // Lưu kết quả debug
                combinedLines.ToBitmap().Save(@"C:\Users\servi\Downloads\__#14_Combine_line_bitwise.jpg", ImageFormat.Jpeg);
            }

            var threshold = combinedLineBitwise.CopyBlank();
            CvInvoke.Threshold(combinedLineBitwise, threshold, 100, 255, ThresholdType.Binary | ThresholdType.Otsu);
            if (IsDebug)
            {
                // Lưu kết quả debug
                threshold.ToBitmap().Save(@"C:\Users\servi\Downloads\__#15_Combine_line_bitwise_threshold.jpg", ImageFormat.Jpeg);
            }

            var countours = preProcess.FindAllContours(threshold.Not());

            SortContours(countours);

            return verticalLine;
        }

        public Image<Gray, byte> ExtractHoughLines(Image<Bgr, byte> input)
        {
            RawImage = input;
            var preProcess = new Pre();
            var step1BinaryImage = preProcess.ConvertToBinary(input);
            var step2BinaryInvertImage = preProcess.ConvertToInvertBinary(step1BinaryImage);

            var edges = step2BinaryInvertImage.Canny(50, 150);
            if (IsDebug)
            {
                // Lưu kết quả debug
                input.ToBitmap().Save(@"C:\Users\servi\Downloads\__#9_canny.jpg", ImageFormat.Jpeg);
            }
            var lines = edges.HoughLinesBinary(1, Math.PI / 30, 15, 50, 10);

            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    var currentLine = lines[i][j];
                    //if (currentLine.Length > 20)
                    //{

                    //}
                    //else
                    //{
                    //    input.Draw(currentLine, new Bgr(Color.Violet), 2);
                    //}
                    input.Draw(currentLine, new Bgr(Color.Green), 2);
                }
            }

            if (IsDebug)
            {
                // Lưu kết quả debug
                input.ToBitmap().Save(@"C:\Users\servi\Downloads\__#10_binary.jpg", ImageFormat.Jpeg);
            }

            return edges;
        }

        public void SortContours(VectorOfVectorOfPoint contours, string method = "LTR")
        {
            // construct the list of bounding boxes and sort them from top to bottom
            var boudingBoxes = new List<Rectangle>();

            var imgClone = RawImage.Clone();

            for (int k = 0; k < contours.Size; k++)
            {
                //var contourCountArea = CvInvoke.ContourArea(contours[k]);

                var rect = CvInvoke.BoundingRectangle(contours[k]);

                if (rect.Width < 20 || rect.Height < 20 || rect.Width > RawImage.Width / 2 || rect.Height > RawImage.Height / 2)
                {
                    continue;
                }
                boudingBoxes.Add(rect);

                if (IsDebug)
                {
                    // Vẽ viền bao vào ảnh gốc
                    imgClone.Draw(rect, new Bgr(0, 255, 0), 1);
                    // Điểm giữa
                    var centerPoint = new Point(rect.X + rect.Width / 2 - 20, rect.Y + rect.Height / 2);
                    CvInvoke.PutText(imgClone, $"No.{k + 1}", centerPoint, FontFace.HersheySimplex, 0.4, new MCvScalar(0, 0, 255), 2);
                }

            }

            if (IsDebug)
            {
                // Lưu kết quả debug
                imgClone.ToBitmap().Save(@"C:\Users\servi\Downloads\__#16_clean_contours_random_sort.jpg", ImageFormat.Jpeg);
            }

            // Sắp xếp sơ bộ lại trật tự
            boudingBoxes = boudingBoxes
                .OrderBy(x => x.Y)
                .ThenBy(x => x.X).ToList();

            // Đếm số hàng
            var numberOfRows = (int)Math.Ceiling((decimal)boudingBoxes.Count / NumberOfColums);

            // Biến chứa các ô sau khi sắp xếp
            Rectangle[][] sortedBoudingBoxes = new Rectangle[numberOfRows][];

            for (int m = 0; m < sortedBoudingBoxes.Length; m++)
            {
                sortedBoudingBoxes[m] = boudingBoxes.GetRange(m * NumberOfColums, NumberOfColums).OrderBy(x => x.X).ToArray();
            }

            if (IsDebug)
            {
                var counter = 1;
                var imgClone2 = RawImage.Clone();
                for (int m = 0; m < sortedBoudingBoxes.Length; m++)
                {
                    for (int n = 0; n < sortedBoudingBoxes[m].Length; n++)
                    {
                        var currentRect = sortedBoudingBoxes[m][n];

                        // Vẽ viền bao vào ảnh gốc
                        imgClone2.Draw(currentRect, new Bgr(0, 255, 0), 1);
                        // Điểm giữa
                        var centerPoint = new Point(currentRect.X + currentRect.Width / 2 - 30, currentRect.Y + currentRect.Height / 2);
                        CvInvoke.PutText(imgClone2, $"No.{counter++}", centerPoint, FontFace.HersheySimplex, 0.4, new MCvScalar(0, 0, 255), 1);
                        //CvInvoke.PutText(imgClone2, $"X: {currentRect.X}", new Point(currentRect.X, currentRect.Y + 20), FontFace.HersheySimplex, 0.4, new MCvScalar(0, 0, 255), 1);
                    }
                }

                // Lưu kết quả debug
                imgClone2.ToBitmap().Save(@"C:\Users\servi\Downloads\__#17_clean_contours_sorted.jpg", ImageFormat.Jpeg);
            }


        }
    }
}
