using System;
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
        IPatientService patientsService;
        ISettingsService settingsService;
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

            this.dataConnector = GeneActivDeviceConnector.Instance;
            this.databaseConnector = HowRYouBirgitteConnector.Instance;
            this.algorithm = new HundredAlgorithm(); // new SimpleAlgorithm();

            this.availableDevices = new ObservableCollection<DeviceData>();
            foreach (var v in dataConnector.GetConnectedDevices())
            {
                DeviceInformation device = (DeviceInformation)v;
                var rec = new FilesDeviceData(device);
                rec.Connected = true;
                RunOnMain(() => availableDevices.Add(rec));
            }
            this.dataConnector.GetConnectedDevices().CollectionChanged += ConnectedDevicesChangeHandler;
        }

        private ObservableCollection<DeviceData> availableDevices = null;
        public ObservableCollection<DeviceData> GetAvailableDevices()
        {
            return this.availableDevices;
        }

        private FilesDeviceData FindDevice(Guid deviceId)
        {
            FilesDeviceData result = (FilesDeviceData)this.availableDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            return result;
        }

        const string PATH = @"c:\SmartWatch\data\";

        public void GetData(Guid deviceId)
        {
            FilesDeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DeviceInserted) return;
            if (!device.Connected) return;

            device.CurrentStep = DataProcessStep.Processing;
            OnPropertyChanged("CurrentStep");

            if (Directory.Exists(PATH + device.DeviceId))
            { // Delete if the directory already exists
                Directory.Delete(PATH + device.DeviceId, true);
            }

            Directory.CreateDirectory(PATH + device.DeviceId + @"\temp"); // Create the directory

            dataConnector.ProgressUpdate += device.Notify;
            try
            {
                device.PathCsv = dataConnector.DownloadData(deviceId, PATH + device.DeviceId);
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId);  // reset progress bar
                device.CurrentStep = DataProcessStep.DeviceInserted;
                OnPropertyChanged("CurrentStep");
                throw ex;
            }
            finally
            {
                dataConnector.ProgressUpdate -= device.Notify;
            }

            string[] info = Path.GetFileNameWithoutExtension(device.PathCsv).Split('_');
            device.Date = info[1];

            device.CurrentStep = DataProcessStep.DataDownloaded;
            OnPropertyChanged("CurrentStep");
        }

        public void ProcessData(Guid deviceId)
        {
            FilesDeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataDownloaded) return;

            device.Notify(-1, deviceId);  // -1 is indeterminate
            device.CurrentStep = DataProcessStep.Processing;
            OnPropertyChanged("CurrentStep");

            try
            {
                string zipName = Path.GetFileNameWithoutExtension(device.PathCsv) + ".zip";
                device.PathZip = zipFile(PATH + device.DeviceId, zipName);
                this.algorithm.ProgressUpdate += device.Notify;
                device.ViewData = this.algorithm.ProcessDataFromFile(device.PathCsv, deviceId);
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId);  // reset progress bar
                device.CurrentStep = DataProcessStep.DataDownloaded;
                OnPropertyChanged("CurrentStep");
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

        public void SendData(Guid deviceId)
        {
            FilesDeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (device.CurrentStep != DataProcessStep.DataProcessed) return;

            device.Notify(-1, deviceId); // -1 is indeterminate
            device.CurrentStep = DataProcessStep.Processing;
            OnPropertyChanged("CurrentStep");

            string password;
            try
            {
                password = patientsService.GetPassword(device.Username);

                var token = databaseConnector.Login(device.Username, password);
                databaseConnector.UploadFile(device.PathZip, token);
                databaseConnector.UploadViewData(device.ViewData, device.Date, token);
                databaseConnector.Logout(token);

            }
            catch (KeyNotFoundException)
            {
                device.Notify(0, deviceId); // reset progress bar
                device.CurrentStep = DataProcessStep.DataProcessed;
                OnPropertyChanged("CurrentStep");
                throw new MyExceptions.UnknownPatientException();
            }
            catch (Exception ex)
            {
                device.Notify(0, deviceId); // reset progress bar
                device.CurrentStep = DataProcessStep.DataProcessed;
                OnPropertyChanged("CurrentStep");
                throw ex;
            }

            device.Notify(100, deviceId);// to show we are done

            device.CurrentStep = DataProcessStep.DataUploaded;
            OnPropertyChanged("CurrentStep");
        }

        public void PrepareDevice(Guid deviceId, string username)
        {
            FilesDeviceData device = FindDevice(deviceId);
            if (device == null) return;
            if (!device.Connected) return;

            device.Notify(-1, deviceId);
            device.CurrentStep = DataProcessStep.Processing;
            OnPropertyChanged("CurrentStep");

            dataConnector.SetupDevice(deviceId, username, settingsService.Settings);

            // Determine whether the directory exists
            if (Directory.Exists(PATH + device.DeviceId))
            {
                Directory.Delete(PATH + device.DeviceId, true);
            }

            device.Notify(100, deviceId); // to show we are done

            device.CurrentStep = DataProcessStep.DeviceCleared;
            OnPropertyChanged("CurrentStep");
        }

        private void RunOnMain(Action function)
        {
            Application.Current.Dispatcher.Invoke(function);
        }

        private static string zipFile(string path, string targetfileName)
        {
            string tempFolderPath = path + @"\temp";
            string zipFilePath = path + @"\" + targetfileName;
            if (!Directory.Exists(tempFolderPath))
            {
                throw new Exception("Cannot read the loaded file in temporary folder!");
            }
            ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

            return zipFilePath;
        }

        private void ConnectedDevicesChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var v in e.NewItems)
                {
                    DeviceInformation newDevice = (DeviceInformation)v;
                    var rec = new FilesDeviceData(newDevice);
                    rec.Connected = true;
                    RunOnMain(() => availableDevices.Add(rec));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var v in e.OldItems)
                {
                    DeviceInformation oldDevice = (DeviceInformation)v;
                    RunOnMain(() =>
                    {
                        DeviceData deviceData = this.FindDevice(oldDevice.DeviceId);

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


        private class FilesDeviceData : DeviceData, INotifyPropertyChanged
        {
            public FilesDeviceData(DeviceInformation deviceInfo)
                : base(deviceInfo)
            { }

            public string PathCsv { get; set; }
            public string PathZip { get; set; }

        }
    }
}
