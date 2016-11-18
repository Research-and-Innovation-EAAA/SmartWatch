using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    interface IDataConnector
    {
        string DownloadData(Guid deviceId, string path);
        //TODO how to model a singleton? - cannot

        ObservableCollection<ListViewDeviceItem> GetConnectedDevices();
    }
}
