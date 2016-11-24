using IoTDataReceiver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    public interface IDataReceiver
    {

        void GetData(Guid deviceId);

        void ProcessData();

        void SendData();

        void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings);

        ObservableCollection<ListViewDeviceItem> GetConnectedDevices();

        event ProgressUpdateHandler ProgressUpdate;

        DataProcessStep CurrentStep { get; }

    }
}
