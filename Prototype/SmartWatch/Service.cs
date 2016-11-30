﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace IoTDataReceiver
{
    public class Service : IService, INotifyPropertyChanged
    {
        PatientService patientsService;
        SettingsService settingsService;
        IDeviceConnector dataConnector;
        IDatabaseConnector databaseConnector;
        IProcessAlgorithm algorithm;

        #region Singleton

        private static Service instance = null;
        public static Service Instance
        {
            get
            {
                if (instance == null)
                    instance = new Service();
                return instance;
            }
        }

        #endregion

        private Service()
        {
            this.patientsService = PatientService.Instance;
            this.settingsService = SettingsService.Instance;

            this.dataConnector = GeneActivDeviceConnector.Instance; //DummyDataConnector.Instance;
            this.databaseConnector = HowRYouConnector.Instance;
            this.algorithm = new HundredAlgorithm(); // new SimpleAlgorithm();

            this.availableDevices = new ObservableCollection<IDeviceData>();
            foreach (var v in dataConnector.GetConnectedDevices())
            {
                ListViewDeviceItem device = (ListViewDeviceItem)v;
                var rec = new DeviceData(device.DeviceId, device);
                rec.Connected = true;
                runOnMain(() => availableDevices.Add(rec));
            }
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
                    var rec = new DeviceData(newDevice.DeviceId, newDevice);
                    rec.Connected = true;
                    runOnMain(() => availableDevices.Add(rec));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var v in e.OldItems)
                {
                    ListViewDeviceItem oldDevice = (ListViewDeviceItem)v;
                    runOnMain(() =>
                    {
                        IDeviceData deviceData = this.FindDevice(oldDevice.DeviceId);

                        // if ready for another patient or data processed, remove from list... 
                        if (deviceData.CurrentStep == DataProcessStep.DeviceCleared
                            || deviceData.CurrentStep == DataProcessStep.DataUploaded)
                        {
                            availableDevices.Remove(deviceData);
                        }
                        // otherwise, only mark as disconnected, so it is possible to still process and send the data
                        deviceData.Connected = false;
                    });
                }
            }
        }

        private ObservableCollection<IDeviceData> availableDevices = null;
        public ObservableCollection<IDeviceData> GetAvailableDevices()
        {
            return this.availableDevices;
        }

        private DeviceData FindDevice(Guid deviceId)
        {
            DeviceData result = (DeviceData)this.availableDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            return result;
        }

        const string PATH = @"c:\SmartWatch\data\";

        public void GetData(Guid deviceId)
        {
            DeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DeviceInserted) return;
            if (!device.Connected) return;

            // Determine whether the directory exists
            if (Directory.Exists(PATH + device.DeviceId))
            {
                DirectoryInfo di = new DirectoryInfo(PATH + device.DeviceId);

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
            Directory.CreateDirectory(PATH + device.DeviceId + @"\temp");

            dataConnector.ProgressUpdate += device.Notify;
            try
            {
                device.PathCsv = dataConnector.DownloadData(deviceId, PATH + device.DeviceId);
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId);  // reset progress bar
                throw ex;
            }
            finally
            {
                dataConnector.ProgressUpdate -= device.Notify;
            }

            string[] info = Path.GetFileNameWithoutExtension(device.PathCsv).Split('_');
            device.Username = info[0];
            device.Date = info[1];

            device.CurrentStep = DataProcessStep.DataDownloaded;
            OnPropertyChanged("CurrentStep");
        }

        public void ProcessData(Guid deviceId)
        {
            DeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataDownloaded) return;

            device.Notify(-1, deviceId);  // -1 is indeterminate

            //            System.Threading.Thread.Sleep(5000);

            try
            {
                device.PathZip = zipFile(PATH + device.DeviceId);
                this.algorithm.ProgressUpdate += device.Notify;
                device.ViewData = this.algorithm.ProcessDataFromFile(device.PathCsv, deviceId);
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId);  // reset progress bar
                throw ex;
            }
            finally
            {
                this.algorithm.ProgressUpdate -= device.Notify;
            }

            device.Notify(100, deviceId); // to show we are done

            device.CurrentStep = DataProcessStep.DataProcessed;
            OnPropertyChanged("CurrentStep");
        }

        private static string zipFile(string path)
        {
            string tempFolderPath = path + @"\temp";
            string zipFilePath = path + @"\data.zip";
            if (!Directory.Exists(tempFolderPath))
            {
                throw new Exception("Cannot read the loaded file in temporary folder!");
            }
            ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

            return zipFilePath;
        }

        public void SendData(Guid deviceId)
        {
            DeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataProcessed) return;

            device.Notify(-1, deviceId); // -1 is indeterminate

            string password;
            try
            {
                password = patientsService.GetPassword(device.Username);

                var token = databaseConnector.Login(device.Username, password);
                databaseConnector.UploadFile(device.PathZip, token);
                databaseConnector.UploadViewData(device.ViewData, device.Date, token);
                databaseConnector.Logout(token);

            }
            catch (KeyNotFoundException ex)
            {
                device.Notify(0, deviceId); // reset progress bar
                throw new MyExceptions.UnknownPatientException();
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId); // reset progress bar
                throw ex;
            }

            device.Notify(100, deviceId);// to show we are done

            device.CurrentStep = DataProcessStep.DataUploaded;
            OnPropertyChanged("CurrentStep");
        }

        public void PrepareDevice(Guid deviceId, string username)
        {
            DeviceData device = FindDevice(deviceId);
            if (device == null) return;
            //if (device.currentStep != DataProcessStep.DataUploaded) return;
            if (!device.Connected) return;

            device.Notify(-1, deviceId);

            dataConnector.SetupDevice(deviceId, username, settingsService.Settings);
            //TODO delete files

            device.Notify(100, deviceId); // to show we are done

            device.CurrentStep = DataProcessStep.DeviceCleared;
            OnPropertyChanged("CurrentStep");
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


        public class DeviceData : IDeviceData, INotifyPropertyChanged
        {
            public DeviceData(Guid deviceId, ListViewDeviceItem deviceInfo)
            {
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

            private bool connected;
            public bool Connected
            {
                get { return this.connected; }
                set { this.connected = value; OnPropertyChanged("Connected"); }
            }

            public ListViewDeviceItem DeviceInfo { get; }
            public string PathCsv { get; set; }
            public string PathZip { get; set; }
            public string ViewData { get; set; }
            public string Username { get; set; }
            public string Date { get; set; }


            public void Notify(int progress, Guid deviceId)
            {
                if (!deviceId.Equals(this.deviceId))
                    return;
                this.Progress = progress;
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
