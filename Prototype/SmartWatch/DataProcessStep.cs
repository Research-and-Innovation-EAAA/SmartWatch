using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    public enum DataProcessStep
    {
        WatchInserted = 0,
        DataDownloaded = 1,
        DataProcessed = 2,
        DataUploaded = 3,
        WatchCleared = 4
    }
}
