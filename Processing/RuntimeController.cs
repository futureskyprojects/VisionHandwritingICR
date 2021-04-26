using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VisionHandwritingICR.Processing
{
    public static class RuntimeController
    {
        public static string GetTesseractViModelDirectory()
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "TesseractModels");
            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }
            return currentAppPath;
        }

        public static void SaveTesseractViModelToRuntimeDirectory()
        {
            var currentAppPath = Path.Combine(GetTesseractViModelDirectory(), "vie.traineddata");
            if (!File.Exists(currentAppPath))
            {
                File.WriteAllBytes(currentAppPath, Properties.Resources.vie);
            }
        }


        public static void CurrentSave(this Bitmap bmp, string fileName, string folder = "")
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "LatestProcessed");
            if (!string.IsNullOrEmpty(folder))
            {
                currentAppPath = Path.Combine(currentAppPath, folder);
            }

            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }
            currentAppPath = Path.Combine(currentAppPath, fileName);
            bmp.Save(currentAppPath, ImageFormat.Jpeg);
        }

        public static void CleanFolder(string folder = "")
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "LatestProcessed");
            if (!string.IsNullOrEmpty(folder))
            {
                currentAppPath = Path.Combine(currentAppPath, folder);
            }

            if (Directory.Exists(currentAppPath))
            {
                Directory.Delete(currentAppPath, true);
            }
        }
    }
}
