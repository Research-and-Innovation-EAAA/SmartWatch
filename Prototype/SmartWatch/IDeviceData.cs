using System;

namespace IoTDataReceiver
{
    public interface IDeviceData
    {
        Guid DeviceId { get; }
        DataProcessStep CurrentStep { get; set; }
        int Progress { get; }
        ListViewDeviceItem DeviceInfo { get; }
        string ViewData { get; set; }
        string Username { get; set; }
        string Date { get; set; }
        bool Connected { get; set; }

        void Notify(int progress, Guid deviceId);
    }
}
