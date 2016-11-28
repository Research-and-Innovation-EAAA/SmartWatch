using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    public class DataReceiver : IDataReceiver, INotifyPropertyChanged
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
            this.dataConnector = new DummyDataConnector(); //new GeneActivDataConnector();
            this.algorithm = new DummyAlgorithm();
            this.patients = PatientService.Instance;
            this.howRYou = HowRYouConnector.Instance;
            this.availableDevices = new ObservableCollection<DeviceReceiver>();

            this.dataConnector.GetConnectedDevices().CollectionChanged += ConnectedDevicesChangeHandler;
        }

        private void runOnMain(Action function)
        {
            Application.Current.Dispatcher.Invoke(function);
        }

        private void ConnectedDevicesChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var v in e.NewItems)
                {
                    ListViewDeviceItem newDevice = (ListViewDeviceItem)v;
                    Debug.WriteLine("adding!!!!");
                    var rec = new DeviceReceiver(this, newDevice.DeviceId, newDevice);
                    runOnMain(() => availableDevices.Add(rec));
                    Debug.WriteLine("adding!!!!DONE");
                }
            }
        }

        private ObservableCollection<DeviceReceiver> availableDevices = null;

        public ObservableCollection<DeviceReceiver> GetAvailableDevices()
        {
            return this.availableDevices;
        }

        private DeviceReceiver FindDevice(Guid deviceId)
        {
            DeviceReceiver result = this.availableDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            return result;

        }

        const string PATH = @"c:\SmartWatch\realtest\";
        private string pathCsv, pathZip;
        private string viewData;
        private string username, date;


        public void GetData(Guid deviceId)
        {
            DeviceReceiver device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DeviceInserted) return;

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

            dataConnector.ProgressUpdate += device.Notify;
            this.pathCsv = dataConnector.DownloadData(deviceId, PATH);
            dataConnector.ProgressUpdate -= device.Notify;



            string[] info = Path.GetFileNameWithoutExtension(pathCsv).Split('_');
            this.username = info[0];
            this.date = info[1];

            device.CurrentStep = DataProcessStep.DataDownloaded;
            OnPropertyChanged("CurrentStep");
        }

        public void ProcessData(Guid deviceId)
        {
            DeviceReceiver device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataDownloaded) return;

            device.Notify(-1);
            //     OnProgressUpdate(-1);

            System.Threading.Thread.Sleep(5000);
            //        this.pathZip = zipFile(PATH);
            this.algorithm.ProgressUpdate += device.Notify;
            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv);
            this.algorithm.ProgressUpdate -= device.Notify;

            device.Notify(100);
            //           OnProgressUpdate(100); // to show we are done

            device.CurrentStep = DataProcessStep.DataProcessed;
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

        public void SendData(Guid deviceId)
        {
            DeviceReceiver device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataProcessed) return;

            device.Notify(-1);
            //        OnProgressUpdate(-1);

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

            device.Notify(100);
            //      OnProgressUpdate(100); // to show we are done

            device.CurrentStep = DataProcessStep.DataUploaded;
            OnPropertyChanged("CurrentStep");
        }

        public void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {
            DeviceReceiver device = FindDevice(deviceId);
            if (device == null) return;
            //  if (currentStep != DataProcessStep.DataUploaded) return;

            device.Notify(-1);
            //          OnProgressUpdate(-1);

            dataConnector.SetupDevice(deviceId, username, settings);

            device.Notify(100);
            //          OnProgressUpdate(100); // to show we are done

            device.CurrentStep = DataProcessStep.DeviceCleared;
            OnPropertyChanged("CurrentStep");
        }

        public DataProcessStep? GetCurrentStep(Guid deviceId)
        {
            DeviceReceiver device = FindDevice(deviceId);
            if (device == null) return null;
            return device.CurrentStep;
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

        #region OnProgressUpdate

        public event ProgressUpdateHandler ProgressUpdate;
        protected virtual void OnProgressUpdate(int progress)
        {
            ProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress);
        }

        #endregion

        public class DeviceReceiver : INotifyPropertyChanged
        {
            DataReceiver dataReceiver;
            public DeviceReceiver(DataReceiver dataReceiver, Guid deviceId, ListViewDeviceItem deviceInfo)
            {
                this.dataReceiver = dataReceiver;
                this.deviceId = deviceId;
                this.DeviceInfo = deviceInfo;
                this.CurrentStep = DataProcessStep.DeviceInserted;
            }

            private Guid deviceId;

            public Guid DeviceId
            {
                get { return this.deviceId; }
            }

            private DataProcessStep currentStep;

            public DataProcessStep CurrentStep
            {
                get { return this.currentStep; }
                set { this.currentStep = value; OnPropertyChanged("CurrentStep"); }
            }

            private int progress;

            public int Progress
            {
                get { return this.progress; }
                set { this.progress = value; OnPropertyChanged("Progress"); }
            }

            public ListViewDeviceItem DeviceInfo { get; }

            public void Notify(int progress)
            {
                this.Progress = progress;
                //     dataReceiver.Notify(progress);
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
}
