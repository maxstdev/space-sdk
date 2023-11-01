using System.IO;
using System.Text;
using UnityEngine;

namespace MaxstXR.Place
{
    public class TopologyDataHelper
    {
        private const string FILE_NAME_PREFIX = "Topology_";

        public static string LoadData(long spotId)
        {
            var fileName = GetFilePath(spotId);
            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName, Encoding.UTF8);
            }
            return null;
        }

        public static void SaveData(string data, long spotId)
        {
            string fileName = GetFilePath(spotId);
            File.WriteAllText(fileName, data, Encoding.UTF8);
        }

        public static string GetFilePath(long spotId)
        {
            string fileName = $"{FILE_NAME_PREFIX}{spotId}";
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}