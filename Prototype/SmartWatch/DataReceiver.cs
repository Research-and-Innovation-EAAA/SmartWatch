using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace IoTDataReceiver
{
    class DataReceiver : IDataReceiver
    {
        IDataConnector dataConnector;
        IProcessAlgorithm algorithm;
        PatientService patients;
        HowRYouConnector howRYou;

        public DataReceiver() {
            this.dataConnector = new GeneActivDataConnector();
            this.algorithm = new DummyAlgorithm();
            this.patients = new PatientService();
            this.howRYou = HowRYouConnector.Instance;
            this.status = 0;
        }

        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices()
        {
            return dataConnector.GetConnectedDevices();
        }

        private int status; // 0 not, 1 loaded, 2 processed, 3 sent
        public int getStatus() { return this.status; }

        const string PATH = @"c:\SmartWatch\realtest\";
        private string pathCsv, pathZip;
        private string viewData;
        private string username, date;

        public void GetData(Guid deviceId)
        {
            if (status != 0) return;

            this.pathCsv = dataConnector.DownloadData(deviceId, PATH);

            string[] info = Path.GetFileNameWithoutExtension(pathCsv).Split('_');
            this.username = info[0];
            this.date = info[1];

            status = 1;
        }

        public void ProcessData()
        {
            if (status != 1) return;

            this.pathZip = zipFile(PATH);

            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv);

            status = 2;
        }

        private static string zipFile(string path) {

            string startPath = @"c:\example\start";
            string zipPath = @"c:\example\result.zip";
          

            ZipFile.CreateFromDirectory(startPath, zipPath);


            string tempFolderPath = path + @"temp";
            string zipFilePath = path + @"data.zip";
            if (!Directory.Exists(tempFolderPath))
            {
                throw new Exception("Cannot read the loaded file in temporary folder!");
            }
            ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath); // TODO throwing errors all the time
            //ZipFile.CreateFromDirectory(@"c:\SmartWatch\realtest\temp\", @"c:\SmartWatch\file.zip");
            return zipFilePath;
        }

        public void SendData()
        {
            if (status != 2) return;

            string password = patients.GetPassword(this.username); 
            var token = howRYou.Login(this.username, password);

            howRYou.UploadFile(this.pathZip, token);

            howRYou.UploadViewData(this.viewData, this.date, token); 

            howRYou.Logout(token);


            status = 3;
            throw new NotImplementedException();
        }
    }
}
