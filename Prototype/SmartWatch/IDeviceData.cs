using System;

namespace IoTDataReceiver
{
    public interface IDeviceData
    {
        Guid DeviceId { get; }
        string Username { get; }
        DataProcessStep CurrentStep { get; set; }
        int Progress { get; }
        DeviceInformation DeviceInfo { get; }
        string ViewData { get; set; }
        string Date { get; set; }
        bool Connected { get; set; }

        void Notify(int progress, Guid deviceId);
    }
}
