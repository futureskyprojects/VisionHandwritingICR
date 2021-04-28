using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionHandwritingICR.Processing
{
    public static class RuntimeController
    {
        public static string GetConfigDirectory()
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "Configs");

            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }
            return currentAppPath;
        }

        public static async Task<string> GetTableHeaderColumnsConfigs()
        {
            var filePath = Path.Combine(GetConfigDirectory(), "column.config.vistark");
            if (!File.Exists(filePath))
            {
                await File.WriteAllBytesAsync(filePath, Properties.Resources.column_config);
            }
            return filePath;
        }

        public static async Task<string[]> GetHeaderColumns()
        {
            var currentPath = await GetTableHeaderColumnsConfigs();
            var columns = await File.ReadAllLinesAsync(currentPath);
            return columns;
        }

        public static string GetExcelExportDirectory()
        {
            var currentAppPath = Path.GetDirectoryName(Application.ExecutablePath);
            currentAppPath = Path.Combine(currentAppPath, "Exports");

            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }
            return currentAppPath;
        }

        public static string GetExcelExportNewFileName(string prefixFileName = "KetQuaDiem")
        {
            if (Regex.IsMatch(prefixFileName, @"KetQuaDiem_\d+"))
            {
                prefixFileName = prefixFileName.Split("_")[0];
            }

            var currentPath = GetExcelExportDirectory();
            DirectoryInfo d = new DirectoryInfo(currentPath);
            FileInfo[] files = d.GetFiles("*.xlsx");

            var latestIndex = -1;

            foreach (var f in files)
            {
                var currentName = Path.GetFileNameWithoutExtension(f.FullName);
                var splits = currentName.Split("_");
                if (splits.Length <= 2)
                {
                    if (splits[0].Equals(prefixFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (splits.Length == 2)
                        {
                            var res = -1;
                            if (int.TryParse(splits[1], out res) && res > latestIndex)
                            {
                                latestIndex = res;
                            }
                        }
                    }
                }
            }

            return Path.Combine(currentPath, prefixFileName + ".xlsx");
        }

        public static string GetKeyDirectory()
        {
            var filePath = Path.GetDirectoryName(Application.ExecutablePath);
            filePath = Path.Combine(filePath, "APIKey");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            return filePath;
        }

        public static string GetAPIKeyFilePath()
        {
            var filePath = Path.Combine(GetKeyDirectory(), "app_key.json");
            return filePath;
        }

        public static async Task SaveAPIKeyFile()
        {
            var filePath = Path.Combine(GetKeyDirectory(), "app_key.json");
            if (!File.Exists(filePath))
            {
                await File.WriteAllBytesAsync(filePath, Properties.Resources.app_key);
            }
        }

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

        public static async Task SaveTesseractViModelToRuntimeDirectory()
        {
            var currentAppPath = Path.Combine(GetTesseractViModelDirectory(), "vie.traineddata");
            if (!File.Exists(currentAppPath))
            {
                await File.WriteAllBytesAsync(currentAppPath, Properties.Resources.vie);
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
