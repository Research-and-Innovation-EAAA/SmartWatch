using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    class GeneActivDataConnector : IDataConnector
    {

        private static GeneActivDataConnector instance;

        public static GeneActivDataConnector GetInstance()
        {
            if (instance == null)
            {
                instance = new GeneActivDataConnector();
            }
            return instance;
        }

        private GeneActivDataConnector() { }

        public string DownloadData(IGeneaDevice device)
        {
            const string PATH = @"c:\SmartWatch\test\";

            // Determine whether the directory exists.
            if (Directory.Exists(PATH))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(PATH);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

            }

            Directory.CreateDirectory(PATH + "temp");

            GeneaDateTime startTime = device.ReadData(1, 1)[0].DataHeader.PageTime;
            string startTimeUtc = startTime.ToDateTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
            string fileName = PATH + @"temp\" + device.SubjectInfo.SubjectCode + "_" + startTime.ToDateTime().ToString("yyyyMMddHHmmss") + ".csv";

            using (var filer = new GeneaDeviceFiler(device, fileName))
            {
                filer.ExtractOperatorID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                filer.ExtractNotes = "Downloaded by IoTDataReceiver";

                //        filer.WriteDataProgress += _OnExtractProgress; TODO progressbar
                filer.CreateFile();
                filer.WriteStoredData();
                filer.CloseFile();
                //      filer.WriteDataProgress -= _OnExtractProgress; 

                return fileName;
            }
        }
    }
}