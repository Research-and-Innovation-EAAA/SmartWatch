using System.Collections.Generic;

namespace IoTDataReceiver
{
    interface ISettingsDao
    {

        Dictionary<string, string> Settings { get; set; }

    }
}
