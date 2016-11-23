using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace IoTDataReceiver
{
    class DataReceiver : ProgressSubject, IDataReceiver, IProgressObserver
    {
        IDataConnector dataConnector;
        IProcessAlgorithm algorithm;
        PatientService patients;
        HowRYouConnector howRYou;

        private static DataReceiver instance = null;
        public static DataReceiver Instance
        {
            get
            {
                if (instance == null)
                    instance = new DataReceiver();
                return instance;
            }
        }

        private DataReceiver()
        {
            this.dataConnector = new GeneActivDataConnector();
            this.algorithm = new DummyAlgorithm();
            this.patients = PatientService.Instance;
            this.howRYou = HowRYouConnector.Instance;
            this.status = 0;
        }

        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices()
        {
            return dataConnector.GetConnectedDevices();
        }

        private int status; // 0 not, 1 loaded, 2 processed, 3 sent
        public int GetStatus() { return this.status; }

        const string PATH = @"c:\SmartWatch\realtest\";
        private string pathCsv, pathZip;
        private string viewData;
        private string username, date;

        public void GetData(Guid deviceId)
        {
            if (status != 0) return;

            // Determine whether the directory exists
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

            // Try to create the directory
            Directory.CreateDirectory(PATH + "temp");

            ((ProgressSubject)dataConnector).RegisterObserver(this);
            this.pathCsv = dataConnector.DownloadData(deviceId, PATH);
            ((ProgressSubject)dataConnector).UnregisterObserver(this);



            string[] info = Path.GetFileNameWithoutExtension(pathCsv).Split('_');
            this.username = info[0];
            this.date = info[1];

            status = 1;
        }

        public void ProcessData()
        {
            if (status != 1) return;

            base.NotifyObservers(-1);
            this.pathZip = zipFile(PATH);

            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv, test);
            base.NotifyObservers(100); // to show we are done

            status = 2;
        }

        private void test(int progress)
        {
            Debug.WriteLine("DELEGATE : " + progress);
            base.NotifyObservers(progress);
        }

        private static string zipFile(string path)
        {

            string tempFolderPath = path + @"temp";
            string zipFilePath = path + @"data.zip";
            if (!Directory.Exists(tempFolderPath))
            {
                throw new Exception("Cannot read the loaded file in temporary folder!");
            }
            ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

            return zipFilePath;
        }

        public void SendData()
        {
            if (status != 2) return;

            base.NotifyObservers(-1);

            string password = patients.GetPassword(this.username);
            /*         var token = howRYou.Login(this.username, password);
                     howRYou.UploadFile(this.pathZip, token);
                     howRYou.UploadViewData(this.viewData, this.date, token); 
                     howRYou.Logout(token);*/

            base.NotifyObservers(100); // to show we are done

            status = 3;
        }

        public void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {
            //if (status != 3) return;

            base.NotifyObservers(-1);

            dataConnector.SetupDevice(deviceId, username, settings);

            base.NotifyObservers(100); // to show we are done

            status = 4;
        }

        public void Notify(int progress)
        {
            base.NotifyObservers(progress);
        }
    }
}
