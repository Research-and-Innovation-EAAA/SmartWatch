using GeneActiv.DeviceIOLibrary;
using GeneActiv.DotNetLibrary;
using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class GeneActivDeviceConnector : IDeviceConnector
    {
        #region Singleton
        private static GeneActivDeviceConnector instance = null;
        public static GeneActivDeviceConnector Instance
        {
            get
            {
                if (instance == null)
                    instance = new GeneActivDeviceConnector();
                return instance;
            }
        }
        #endregion

        private GeneActivDeviceConnector()
        {
            Logger.AddLogger(new DebugLogger());

            // Default csv export info string (see download/stream to file)
            GeneaCsvFileIO.DefaultApplicationInfo = "SmartWatch";

            devices = new ObservableCollection<DeviceInformation>();

            // Setup and start the Genea Manager
            manager.GeneaDeviceAdded += OnGeneaDeviceAdded;
            manager.GeneaDeviceRemoved += OnGeneaDeviceRemoved;
            manager.ErrorEvent += OnGeneaManagerError;
            manager.BluetoothAutoDiscoveryPeriod = -1; //deactivate BT discovery
            manager.IsBluetoothEnabled = false;
            manager.StartLiveDeviceMonitor();
        }

        public string DownloadData(Guid deviceId, string path)
        {
            IGeneaDevice device = smartWatches[deviceId];

            GeneaDateTime startTime = null;
            try
            {      // read the time of the very first record
                startTime = device.ReadData(1, 1)[0].DataHeader.PageTime; 
            }
            catch (ArgumentException)
            {
                throw new MyExceptions.NoDataException();
            }

            string fileName = path + @"\temp\" + device.SubjectInfo.SubjectCode + "_" + startTime.ToDateTime().ToString("yyyyMMddHHmmss") + ".csv";

            using (var filer = new GeneaDeviceFiler(device, fileName))
            {
                filer.ExtractOperatorID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                filer.ExtractNotes = "Downloaded using SmartWatch";

                filer.WriteDataProgress += this.OnExtractProgress;
                filer.CreateFile();
                try
                {
                    filer.WriteStoredData();
                }
                catch (DeviceIOException)
                {
                    throw new MyExceptions.CommunicationException();
                }
                finally
                {
                    filer.CloseFile();
                    filer.WriteDataProgress -= this.OnExtractProgress;
                }

                return fileName;
            }
        }

        private void OnExtractProgress(object sender, GeneaDeviceFilerProgressEventArgs e)
        {
            int progress = (int)(100 * ((double)e.NumOfDataBlocks / (double)e.TotalDataBlocks));
            OnProgressUpdate(progress, ((GeneaDeviceFiler)sender).Device.GeneaDeviceID);
        }

        private GeneaDeviceManager manager = new GeneaDeviceManager();

        private Dictionary<Guid, IGeneaDevice> smartWatches = new Dictionary<Guid, IGeneaDevice>();

        private ObservableCollection<DeviceInformation> devices = null;
        public ObservableCollection<DeviceInformation> GetConnectedDevices() { return devices; }

        public event DeviceProgressUpdateHandler ProgressUpdate;
        protected virtual void OnProgressUpdate(int progress, Guid deviceId)
        {
            DeviceProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress, deviceId);
        }

        private DeviceInformation FindDeviceListItem(Guid id)
        {
            return devices.FirstOrDefault(d => d.DeviceId == id);
        }

        private void OnGeneaDeviceAdded(object sender, GeneaDeviceAddedEventArgs e)
        {
            IGeneaDevice device = e.GeneaDevice;

            devices.Add(new DeviceInformation
            {
                DeviceId = device.GeneaDeviceID,
                DeviceNumber = device.DeviceIdentity.DeviceUniqueSerialCode,
                PatientName = device.SubjectInfo.SubjectCode
            });

            device.StatusUpdate += OnLiveDeviceStatusUpdate;
            smartWatches.Add(device.GeneaDeviceID, device);

            // Bee beep sound
            Sound.PlayAlias("DeviceConnect", true);
        }

        private void OnGeneaDeviceRemoved(object sender, GeneaDeviceRemovedEventArgs e)
        {
            Guid deviceId = e.GeneaDeviceID;

            smartWatches[deviceId].StatusUpdate -= OnLiveDeviceStatusUpdate;
            smartWatches.Remove(deviceId);

            for (int i = 0; i < devices.Count; i++)
            {
                DeviceInformation item = devices[i];
                if (item.DeviceId == deviceId)
                {
                    devices.Remove(item);
                    break;
                }
            }

            // Bee boop sound
            Sound.PlayAlias("DeviceDisconnect", true);
        }

        private void OnGeneaManagerError(object sender, ErrorLogEventArgs e)
        {
            Logger.WriteErrorLogEvent(e);
        }

        private void OnLiveDeviceStatusUpdate(object sender, GeneaStatusUpdateEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => LiveDeviceStatusUpdate(e.GeneaDeviceID, e.Status))); //TODO battery status
        }

        private void LiveDeviceStatusUpdate(Guid deviceID, GeneaStatus status)
        {
            Debug.Write("device" + deviceID + ", status " + status);
            DeviceInformation item = this.FindDeviceListItem(deviceID);
            if (item != null) item.SetBatteryVoltage(status.BatteryVoltage);  // volts to level
        }

        public void SetupDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {
            IGeneaDevice device = smartWatches[deviceId];
            float frequency = float.Parse(settings["frequency"], CultureInfo.InvariantCulture);
            int period = int.Parse(settings["period"], CultureInfo.InvariantCulture);
            string studyCenter = settings["studyCenter"];
            string studyCode = settings["studyCode"];

            this.SetupDevice(device, username, frequency, period, studyCenter, studyCode);
        }

        private void SetupDevice(IGeneaDevice device, string username, float frequency, int period, string studyCenter, string studyCode)
        {
            GeneaConfigInfoRW deviceConfigInfo = new GeneaConfigInfoRW();
            deviceConfigInfo.ConfigOperatorID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            deviceConfigInfo.ConfigNotes = "Setup by IoTDataReceiver";
            deviceConfigInfo.StartMode = GeneaStartMode.OnButton;
            deviceConfigInfo.MeasurementFrequency = frequency;
            deviceConfigInfo.MeasurementPeriod = period;

            GeneaSubjectInfoRW deviceSubjectInfo = new GeneaSubjectInfoRW();
            deviceSubjectInfo.SubjectCode = username;
            deviceSubjectInfo.SubjectNotes = "";
            // no need to manually null other attributes, they are null by default

            GeneaTrialInfoRW deviceTrialInfo = new GeneaTrialInfoRW();
            deviceTrialInfo.InvestigatorID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            deviceTrialInfo.StudyCentre = studyCenter;
            deviceTrialInfo.StudyCode = studyCode;

            try
            {
                device.SetupDevice(deviceConfigInfo, deviceSubjectInfo, deviceTrialInfo, true);
            }
            catch (GeneaDeviceException ex)
            {
                throw new MyExceptions.DeviceException(ex.Message);
            }
        }
    }
}