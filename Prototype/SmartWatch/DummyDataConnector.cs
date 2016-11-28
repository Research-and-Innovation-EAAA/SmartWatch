﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class DummyDataConnector : IDataConnector
    {

        public DummyDataConnector()
        {
            devices = new ObservableCollection<ListViewDeviceItem>();

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                devices.Add(new ListViewDeviceItem
                {
                    DeviceId = Guid.NewGuid(),
                    PatientName = "Pepik"
                });

                devices.Add(new ListViewDeviceItem
                {
                    DeviceId = Guid.NewGuid(),
                    PatientName = "Mylan"
                });

                await Task.Run(async () =>
                 {
                     await Task.Delay(TimeSpan.FromSeconds(5));
                     devices.Add(new ListViewDeviceItem
                     {
                         DeviceId = Guid.NewGuid(),
                         PatientName = "Monika"
                     });
                 });
            });
        }

        public string DownloadData(Guid deviceId, string path)
        {
            for (int i = 0; i <= 30; i++) // wait loop simulating reading data
            {
                System.Threading.Thread.Sleep(500);
                OnProgressUpdate((int)(i / 30f * 100));
            }

            DateTime startTime = DateTime.Now;
            string fileName = path + @"temp\DUMMY_" + startTime.ToString("yyyyMMddHHmmss") + ".csv";

            return fileName;
        }


        private ObservableCollection<ListViewDeviceItem> devices = null;


        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices() { return devices; }

        private ListViewDeviceItem FindDeviceListItem(Guid id)
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
                OnProgressUpdate((int)(i / 10f * 100));
            }
        }


        public event ProgressUpdateHandler ProgressUpdate;
        protected virtual void OnProgressUpdate(int progress)
        {
            ProgressUpdateHandler handler = ProgressUpdate;
            if (handler != null) handler(progress);
        }
    }
}