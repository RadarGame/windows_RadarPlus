using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Tools
{
    internal static class DirectoryHelperFunctions
    {
        internal static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        internal static async Task WriteAsync(string data, string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                await sw.WriteAsync(data);
            }
        }

    }
}
