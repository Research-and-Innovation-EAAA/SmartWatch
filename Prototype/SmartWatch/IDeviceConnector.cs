﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    interface IDeviceConnector
    {
        /// <summary>
        /// Method for downloading data from a device, returning a CSV file with records.
        /// Req. CSV file must be in format username_yyyyMMddHHmmss.csv, where the date corresponds to the time of the start of measurement.
        /// </summary>
        /// <param name="deviceId">Identifier of the device to download data from</param>
        /// <param name="path">Path to a folder where to create a temp/ with output file</param>
        /// <returns>Path to created CSV file, beware of the naming requirement!</returns>
        string DownloadData(Guid deviceId, string path);

        void SetupDevice(Guid deviceId, string username, Dictionary<string, string> settings);

        ObservableCollection<DeviceInformation> GetConnectedDevices();

        event DeviceProgressUpdateHandler ProgressUpdate;
    }
}
