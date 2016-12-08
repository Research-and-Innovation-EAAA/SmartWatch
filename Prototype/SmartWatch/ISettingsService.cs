using System.Collections.Generic;

namespace IoTDataReceiver
{
    interface ISettingsService
    {
        Dictionary<string, string> Settings { get; set; }
    }
}
