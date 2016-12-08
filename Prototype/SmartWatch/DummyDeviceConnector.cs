using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class DummyDeviceConnector : IDeviceConnector
    {
        private static DummyDeviceConnector instance = null;
        public static DummyDeviceConnector Instance
        {
            get
            {
                if (instance == null)
                    instance = new DummyDeviceConnector();
                return instance;
            }
        }

        private DummyDeviceConnector()
        {
            devices = new ObservableCollection<DeviceInformation>();

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                DeviceInformation signe = new DeviceInformation
                {
                    DeviceId = Guid.NewGuid(),
                    PatientName = "Signe"
                };
                devices.Add(signe);

                devices.Add(new DeviceInformation
                {
                    DeviceId = Guid.NewGuid(),
                    PatientName = "John"
                });

                await Task.Run(async () => // simulating insertion after 5 sec
                 {
                     await Task.Delay(TimeSpan.FromSeconds(5));
                     devices.Add(new DeviceInformation
                     {
                         DeviceId = Guid.NewGuid(),
                         PatientName = "Simon"
                     });
                 });
                await Task.Run(async () => // simulating disconnecting after 20 sec
                {
                    await Task.Delay(TimeSpan.FromSeconds(20));
                    devices.Remove(signe);
                });
            });
        }

        public string DownloadData(Guid deviceId, string path)
        {
            for (int i = 0; i <= 30; i++) // wait loop simulating reading data
            {
                System.Threading.Thread.Sleep(500);
                OnProgressUpdate((int)(i / 30f * 100), deviceId);
            }

            DateTime startTime = DateTime.Now;
            string fileName = path + @"temp\DUMMY_" + startTime.ToString("yyyyMMddHHmmss") + ".csv";

            return fileName;
        }


        private ObservableCollection<DeviceInformation> devices = null;
        public ObservableCollection<DeviceInformation> GetConnectedDevices() { return devices; }

        private DeviceInformation FindDeviceListItem(Guid id)
        {
            return devices.FirstOrDefault(d => d.DeviceId == id);
        }

        public void SetupDevice(Guid deviceId, string username, Dictionary<string, string> settings)
        {

            float frequency = float.Parse(settings["frequency"], CultureInfo.InvariantCulture);
            int period = int.Parse(settings["period"], CultureInfo.InvariantCulture);
            string studyCenter = settings["studyCenter"];
            string studyCode = settings["studyCode"];

            Debug.WriteLine("SETTING UP: " + frequency + " Hz, " + period + " h., " + studyCenter + " " + studyCode);

            for (int i = 0; i <= 10; i++) // wait loop simulating setting up
            {
                System.Threading.Thread.Sleep(500);
                OnProgressUpdate((int)(i / 10f * 100), deviceId);
            }
        }


        public event DeviceProgressUpdateHandler ProgressUpdate;
        protected virtual void OnProgressUpdate(int progress, Guid deviceId)
        {
            DeviceProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress, deviceId);
        }
    }
}