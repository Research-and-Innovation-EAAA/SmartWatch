using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace IoTDataReceiver
{
    class DataReceiver : ProgressSubject, IDataReceiver, IProgressObserver, INotifyPropertyChanged
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

            ((ProgressSubject)dataConnector).RegisterObserver(this);
            this.pathCsv = dataConnector.DownloadData(deviceId, PATH);
            ((ProgressSubject)dataConnector).UnregisterObserver(this);



            string[] info = Path.GetFileNameWithoutExtension(pathCsv).Split('_');
            this.username = info[0];
            this.date = info[1];

            currentStep = DataProcessStep.DataDownloaded;
            OnPropertyChanged("CurrentStep");
        }

        public void ProcessData()
        {
            if (currentStep != DataProcessStep.DataDownloaded) return;

            base.NotifyObservers(-1);

            this.pathZip = zipFile(PATH);
            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv, test);

            base.NotifyObservers(100); // to show we are done

            currentStep = DataProcessStep.DataProcessed;
            OnPropertyChanged("CurrentStep");
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
            if (currentStep != DataProcessStep.DataProcessed) return;

            base.NotifyObservers(-1);

            string password;
            try
            {
                password = patients.GetPassword(this.username);
            }
            catch (NullReferenceException ex)
            {
                throw new MyExceptions.UnknownPatientException();
            }
            var token = howRYou.Login(this.username, password);
            howRYou.UploadFile(this.pathZip, token);
            howRYou.UploadViewData(this.viewData, this.date, token);
            howRYou.Logout(token);

            base.NotifyObservers(100); // to show we are done

            currentStep = DataProcessStep.DataUploaded;
            OnPropertyChanged("CurrentStep");
        }

        public void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {
          //  if (currentStep != DataProcessStep.DataUploaded) return;

            base.NotifyObservers(-1);

            dataConnector.SetupDevice(deviceId, username, settings);

            base.NotifyObservers(100); // to show we are done

            currentStep = DataProcessStep.WatchCleared;
            OnPropertyChanged("CurrentStep");
        }

        public void Notify(int progress)
        {
            base.NotifyObservers(progress);
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
