using IoTDataReceiver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IoTDataReceiver.DataReceiver;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    public interface IDataReceiver
    {

        void GetData(Guid deviceId);

        void ProcessData(Guid deviceId);

        void SendData(Guid deviceId);

        void PrepareDevice(Guid deviceId, string username, Dictionary<string, string> settings);

        ObservableCollection<DeviceReceiver> GetAvailableDevices();

        event ProgressUpdateHandler ProgressUpdate;

        DataProcessStep? GetCurrentStep(Guid deviceId);

    }
}
