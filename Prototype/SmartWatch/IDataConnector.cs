using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    interface IDataConnector
    {
        string DownloadData(IGeneaDevice device); //TODO I need the device
        //TODO how to model a singleton?
    }
}
