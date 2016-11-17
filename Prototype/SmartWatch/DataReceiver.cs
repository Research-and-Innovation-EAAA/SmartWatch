using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    class DataReceiver : IDataReceiver
    {
        IDataConnector dataConnector;

        public DataReceiver() {
            this.dataConnector = new GeneActivDataConnector();
            this.status = 0;
        }

        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices()
        {
            return dataConnector.GetConnectedDevices();
        }

        private int status; // 0 not, 1 loaded, 2 processed, 3 sent
        public int getStatus() { return this.status; }

        public void GetData(Guid deviceId)
        {
            dataConnector.DownloadData(deviceId);

            status = 1;
            //throw new NotImplementedException();
        }

        public void ProcessData()
        {

            status = 2;
            //throw new NotImplementedException();
        }

        public void SendData()
        {
            status = 3;
            throw new NotImplementedException();
        }
    }
}
