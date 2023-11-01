using UnityEngine;
using System.IO;


namespace MaxstXR.Place
{
    public static class FileController
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB
        }

        public static void SaveFile(string folderPath, string nameWithExtention, byte[] bytes)
        {
            if (!File.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(folderPath + "/" + nameWithExtention))
            {
                var fileStream = new FileStream(folderPath + "/" + nameWithExtention, FileMode.CreateNew);
                var binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.Write(bytes);
                binaryWriter.Close();
                fileStream.Close();
            }
        }

        public static bool CheckFileExist(string filePath)
        {
            bool result = false;

            if (File.Exists(filePath))
            {
                result = true;
            }

            return result;
        }

        public static float ConvertSizeFormat(long byteSize, SizeUnits unit)
        {
            return (float)(byteSize / (double)System.Math.Pow(1024, (long)unit));
        }

        public static string GetSizeFormatString(long byteSize, SizeUnits unit)
        {
            return $"{ConvertSizeFormat(byteSize, unit).ToString("N2")}{unit}";
        }
    }
}