using IoTDataReceiver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    interface IDataReceiver
    {
        void GetData(Guid deviceId);

        void ProcessData();

        void SendData();

        ObservableCollection<ListViewDeviceItem> GetConnectedDevices();

        int getStatus();
    }
}
