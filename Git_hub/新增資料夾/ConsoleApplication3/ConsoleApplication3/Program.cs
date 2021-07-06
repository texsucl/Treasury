using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            string pw = "FBOTEST97041";
            //System.Diagnostics.Process.Start("D:\\Dat\\doEncrypt.bat");
            //System.Diagnostics.Process.Start("D:\\Dat\\doEncrypt.bat", $@"D:\\Dat\\doEncrypt.txt {pw}");

            //System.Diagnostics.Process.Start(@"D:\\Dat\\ENCRYPTION.exe", $@"FBOTEST97041 D:\\Dat\\FBOQ11.txt D:\\Dat\\FBOQ11.txt.enc");

            Process process;
            String command = @"D:\\Dat\\ENCRYPTION.exe";
            ProcessStartInfo processInfo;
            processInfo = new ProcessStartInfo();
            processInfo.FileName = command;
            processInfo.Arguments = "FBOTEST97041 D:\\Dat\\FBOQ11.txt D:\\Dat\\FBOQ11.txt.enc";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);

            process.WaitForExit();
            string datFile = "D:\\Dat";

            string zipFileName = "FBOQ11.zip";

            CreateZipFile(Path.Combine(datFile, zipFileName), new List<string>() { Path.Combine(datFile, "FBOQ11.txt"), Path.Combine(datFile, "FBOQ11.txt.enc") });

            Console.ReadLine();
        }

        /// <summary>
        /// Create a ZIP file of the files provided.
        /// </summary>
        /// <param name="fileName">The full path and name to store the ZIP file at.</param>
        /// <param name="files">The list of files to be added.</param>
        public static void CreateZipFile(string fileName, IEnumerable<string> files)
        {
            // Create and open a new ZIP file
            var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }

    }
}
