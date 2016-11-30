using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IoTDataReceiver
{
    public interface IService
    {

        void GetData(Guid deviceId);

        void ProcessData(Guid deviceId);

        void SendData(Guid deviceId);

        void PrepareDevice(Guid deviceId, string username);

        ObservableCollection<IDeviceData> GetAvailableDevices();

    }
}
