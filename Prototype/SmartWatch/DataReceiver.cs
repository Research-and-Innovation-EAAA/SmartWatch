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
        IProcessAlgorithm algorithm;
        PatientService patients;
        HowRYouConnector howRYou;

        public DataReceiver() {
            this.dataConnector = new GeneActivDataConnector();
            this.algorithm = new DummyAlgorithm();
            this.patients = new PatientService();
            this.howRYou = new HowRYouConnector();
            this.status = 0;
        }

        public ObservableCollection<ListViewDeviceItem> GetConnectedDevices()
        {
            return dataConnector.GetConnectedDevices();
        }

        private int status; // 0 not, 1 loaded, 2 processed, 3 sent
        public int getStatus() { return this.status; }

        private string pathCsv, pathZip;
        private string viewData;

        public void GetData(Guid deviceId)
        {
            if (status != 0) return;

            this.pathCsv = dataConnector.DownloadData(deviceId);

            status = 1;
        }

        public void ProcessData()
        {
            if (status != 1) return;

            this.zipFile(this.pathCsv);

            this.viewData = this.algorithm.ProcessDataFromFile(this.pathCsv);

            status = 2;
        }

        private string zipFile(string path) {
            return "";
        }

        public void SendData()
        {
            if (status != 2) return;

            string password = patients.GetPassword("jkb"); // TODO username?!?!
            var token = howRYou.Login("jkb", password);

            howRYou.UploadFile(this.pathZip, "jkb", token);

            howRYou.UploadViewData(this.viewData, "2016", "jkb", token); // TODO DATE

            howRYou.Logout("jkb", token);


            status = 3;
            throw new NotImplementedException();
        }
    }
}
