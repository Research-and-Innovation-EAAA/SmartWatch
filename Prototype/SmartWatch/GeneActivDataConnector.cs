using GeneActiv.DotNetLibrary;
using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace IoTDataReceiver
{
    class GeneActivDataConnector : IDataConnector
    {

        public GeneActivDataConnector()
        {
            Logger.AddLogger(new DebugLogger()); // can also log to Windows debug log

            // Default csv export info string (see download/stream to file)
            GeneaCsvFileIO.DefaultApplicationInfo = "IoTDataReceiver";

            devices = new ObservableCollection<ListViewDeviceItem>();

            // Setup and start the Genea Manager itself and start looking for BT devices
            manager.GeneaDeviceAdded += OnGeneaDeviceAdded;
            manager.GeneaDeviceRemoved += OnGeneaDeviceRemoved;
            manager.ErrorEvent += OnGeneaManagerError;
            manager.BluetoothAutoDiscoveryPeriod = -1; //deactivate
            manager.IsBluetoothEnabled = false;
            manager.StartLiveDeviceMonitor();
        }

        public string DownloadData(Guid deviceId)
        {
            const string PATH = @"c:\SmartWatch\test\";
            IGeneaDevice device = smartWatches[deviceId];

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

        private GeneaDeviceManager manager = new GeneaDeviceManager();

        private Dictionary<Guid, IGeneaDevice> smartWatches = new Dictionary<Guid, IGeneaDevice>();

        private ObservableCollection<ListViewDeviceItem> devices;
        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices() { return devices; }

        private ListViewDeviceItem FindDeviceListItem(Guid id)
        {
            return devices.FirstOrDefault(d => d.DeviceId == id);
        }

        public void EraseDevice(IGeneaDevice device) { }

        public void SetUpDevice(IGeneaDevice device, float frequency, int period, string subjectCode, string studyCode, string studyCentre) { }

        private void OnGeneaDeviceAdded(object sender, GeneaDeviceAddedEventArgs e)
        {
            //     Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => AddGeneaDevice(e.GeneaDevice)));

            AddGeneaDevice(e.GeneaDevice);
        }

        private void runOnMain(Action function)
        {
            Application.Current.Dispatcher.Invoke(function);
        }

        private void AddGeneaDevice(IGeneaDevice device)
        {
            // New entry for list control data source with default streaming options

            runOnMain(() => { devices.Add(new ListViewDeviceItem { DeviceId = device.GeneaDeviceID, PatientName=device.SubjectInfo.SubjectCode }); });

            device.StatusUpdate += OnLiveDeviceStatusUpdate;
            device.DeviceSetupUpdate += OnLiveDeviceSetupUpdate;
            smartWatches.Add(device.GeneaDeviceID, device);

            // Bee-beep
            Sound.PlayAlias("DeviceConnect", true);
        }

        private void OnGeneaDeviceRemoved(object sender, GeneaDeviceRemovedEventArgs e)
        {
            runOnMain(() => RemoveGeneaDevice(e.GeneaDeviceID));
        }

        private void RemoveGeneaDevice(Guid deviceID)
        {
            smartWatches[deviceID].StatusUpdate -= OnLiveDeviceStatusUpdate;
            //    device.DeviceSetupUpdate -= OnLiveDeviceSetupUpdate; // TODO možná zakomentovat
            smartWatches.Remove(deviceID);

            for (int i = 0; i < devices.Count; i++)
            {
                ListViewDeviceItem item = devices[i];
                if (item.DeviceId == deviceID)
                {
                    devices.Remove(item);
                    break;
                }
            }
            // Bee-boop
            Sound.PlayAlias("DeviceDisconnect", true);
        }

        private void OnGeneaManagerError(object sender, ErrorLogEventArgs e)
        {
            Logger.WriteErrorLogEvent(e);
        }

        private void OnLiveDeviceStatusUpdate(object sender, GeneaStatusUpdateEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => LiveDeviceStatusUpdate(e.GeneaDeviceID, e.Status)));
            //LiveDeviceStatusUpdate(e.GeneaDeviceID, e.Status);
        }

        private void LiveDeviceStatusUpdate(Guid deviceID, GeneaStatus status)
        {
            Debug.Write("device" + deviceID + ", status " + status);
            ListViewDeviceItem item = this.FindDeviceListItem(deviceID);
            if (item != null) item.SetBatteryVoltage(status.BatteryVoltage);  // volts to level
        }

        private void OnLiveDeviceSetupUpdate(object sender, EventArgs e)
        {
            //TODO update listview  Dispatcher.BeginInvoke(new Action(() => CollectionViewSource.GetDefaultView(Devices).Refresh()));
        }

    }
}