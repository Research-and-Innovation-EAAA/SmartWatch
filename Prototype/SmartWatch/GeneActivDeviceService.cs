using GeneActiv.DotNetLibrary;
using GeneActiv.GeneaLibrary;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace IoTDataReceiver
{

    public class GeneActivDeviceService
    {

        private static GeneActivDeviceService instance;

        public static GeneActivDeviceService GetInstance()
        {
            if (instance == null)
            {
                instance = new GeneActivDeviceService();
            }
            return instance;
        }
        
        private GeneActivDeviceService()
        {

            Logger.AddLogger(new DebugLogger()); // can also log to Windows debug log

            // Default csv export info string (see download/stream to file)
            GeneaCsvFileIO.DefaultApplicationInfo = "IoTDataReceiver";

            
                devices = new ObservableCollection<ListViewDeviceItem>();
           

            // Setup and start the Genea Manager itself and start looking for BT devices
            _manager.GeneaDeviceAdded += OnGeneaDeviceAdded;
            _manager.GeneaDeviceRemoved += OnGeneaDeviceRemoved;
            _manager.ErrorEvent += OnGeneaManagerError;
            _manager.BluetoothAutoDiscoveryPeriod = -1; //deactivate
            _manager.IsBluetoothEnabled = false;
            _manager.StartLiveDeviceMonitor();
        }

        private GeneaDeviceManager _manager = new GeneaDeviceManager();

        private ObservableCollection<ListViewDeviceItem> devices;
        public ObservableCollection<ListViewDeviceItem> ConnectedDevices { get { return devices; } }

        private ListViewDeviceItem FindDeviceListItem(Guid id)
        {
            return devices.FirstOrDefault(d => d.Device.GeneaDeviceID == id);
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
            
            runOnMain(() => { devices.Add(new ListViewDeviceItem { Device = device }); });
            
            device.StatusUpdate += OnLiveDeviceStatusUpdate;
            device.DeviceSetupUpdate += OnLiveDeviceSetupUpdate;

            // Bee-beep
            Sound.PlayAlias("DeviceConnect", true);
        }

        private void OnGeneaDeviceRemoved(object sender, GeneaDeviceRemovedEventArgs e)
        {
            RemoveGeneaDevice(e.GeneaDeviceID);
        }

        private void RemoveGeneaDevice(Guid deviceID)
        {
            runOnMain(() =>
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    IGeneaDevice device = devices[i].Device;
                    if (device.GeneaDeviceID == deviceID)
                    {
                        // Disconnect event handlers and dispose of the list object (auto-closes any
                        // files etc. - device itself disposed by manager
                        device.StatusUpdate -= OnLiveDeviceStatusUpdate;
                    //    device.DeviceSetupUpdate -= OnLiveDeviceSetupUpdate; // TODO možná odstranit
                        devices.RemoveAt(i);

                        // Bee-boop
                        Sound.PlayAlias("DeviceDisconnect", true);
                        break;
                    }
                }
            }
            );
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
