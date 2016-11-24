using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    interface IDataConnector
    {
        /// <summary>
        /// Method for downloading data from a device, returning a CSV file with records.
        /// Req. CSV file must be in format username_yyyyMMddHHmmss.csv, where the date corresponds to the time of the start of measurement.
        /// </summary>
        /// <param name="deviceId">Identifier of the device to download data from</param>
        /// <param name="path">Path to a folder where to create a temp/ with output file</param>
        /// <returns>Path to created CSV file, beware of the naming requirement!</returns>
        string DownloadData(Guid deviceId, string path);
        
        event ProgressUpdateHandler ProgressUpdate;

        void SetupDevice(Guid deviceId, string username, Dictionary<string, string> settings);

        ObservableCollection<ListViewDeviceItem> GetConnectedDevices();
    }
}
