using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class DataReceiver : IDataReceiver, INotifyPropertyChanged
    {
        IDataConnector dataConnector;
        IProcessAlgorithm algorithm;
        PatientService patients;
        HowRYouConnector howRYou;

        public event ProgressUpdateHandler ProgressUpdate;
        protected virtual void OnProgressUpdate(int progress)
        {
            ProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress);
        }

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

        public DataProcessStep CurrentStep
        {
            get { return this.currentStep; }
        }

        private DataReceiver()
        {
            this.dataConnector = new GeneActivDataConnector();
            this.algorithm = new DummyAlgorithm();
            this.patients = PatientService.Instance;
            this.howRYou = HowRYouConnector.Instance;
            this.currentStep = DataProcessStep.WatchInserted;
        }

        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices()
        {
            return dataConnector.GetConnectedDevices();
        }

        private DataProcessStep currentStep;
        public DataProcessStep GetCurrentStep() { return this.currentStep; }

        const string PATH = @"c:\SmartWatch\realtest\";
        private string pathCsv, pathZip;
        private string viewData;
        private string username, date;


        public void GetData(Guid deviceId)
        {
            if (currentStep != DataProcessStep.WatchInserted) return;

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

            dataConnector.ProgressUpdate += Notify;
            this.pathCsv = dataConnector.DownloadData(deviceId, PATH);
            dataConnector.ProgressUpdate -= Notify;



            string[] info = Path.GetFileNameWithoutExtension(pathCsv).Split('_');
            this.username = info[0];
            this.date = info[1];

            currentStep = DataProcessStep.DataDownloaded;
            OnPropertyChanged("CurrentStep");
        }

        public void ProcessData()
        {
            if (currentStep != DataProcessStep.DataDownloaded) return;

            OnProgressUpdate(-1);

            this.pathZip = zipFile(PATH);
            this.algorithm.ProgressUpdate += Notify;
            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv);
            this.algorithm.ProgressUpdate -= Notify;

            OnProgressUpdate(100); // to show we are done

            currentStep = DataProcessStep.DataProcessed;
            OnPropertyChanged("CurrentStep");
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
            if (currentStep != DataProcessStep.DataProcessed) return;

            OnProgressUpdate(-1);

            string password;
            try
            {
                password = patients.GetPassword(this.username);
            }
            catch (KeyNotFoundException ex)
            {
                throw new MyExceptions.UnknownPatientException();
            }
            var token = howRYou.Login(this.username, password);
            howRYou.UploadFile(this.pathZip, token);
            howRYou.UploadViewData(this.viewData, this.date, token);
            howRYou.Logout(token);

            OnProgressUpdate(100); // to show we are done

            currentStep = DataProcessStep.DataUploaded;
            OnPropertyChanged("CurrentStep");
        }

        public void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {
            //  if (currentStep != DataProcessStep.DataUploaded) return;

            OnProgressUpdate(-1);

            dataConnector.SetupDevice(deviceId, username, settings);

            OnProgressUpdate(100); // to show we are done

            currentStep = DataProcessStep.WatchCleared;
            OnPropertyChanged("CurrentStep");
        }

        void Notify(int progress)
        {
            OnProgressUpdate(progress);
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// OnPropertyChanged method to raise the event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
