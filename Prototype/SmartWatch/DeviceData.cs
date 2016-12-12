using System;
using System.ComponentModel;

namespace IoTDataReceiver
{
    public class DeviceData : INotifyPropertyChanged
    {
        public DeviceData(DeviceInformation deviceInfo)
        {
            this.DeviceInfo = deviceInfo;
            this.CurrentStep = DataProcessStep.DeviceInserted;
        }

        public Guid DeviceId
        {
            get { return DeviceInfo.DeviceId; }
        }

        private DataProcessStep currentStep;
        public DataProcessStep CurrentStep
        {
            get { return this.currentStep; }
            set { this.currentStep = value; OnPropertyChanged("CurrentStep"); }
        }

        private int progress;
        public int Progress
        {
            get { return this.progress; }
            set { this.progress = value; OnPropertyChanged("Progress"); }
        }

        private bool connected;
        public bool Connected
        {
            get { return this.connected; }
            set { this.connected = value; OnPropertyChanged("Connected"); }
        }

        public string Username
        {
            get { return DeviceInfo.PatientName; }
        }

        public DeviceInformation DeviceInfo { get; }

        private string viewData;
        public string ViewData
        {
            get { return this.viewData; }
            set { this.viewData = value; OnPropertyChanged("ViewData"); }
        }

        private string date;
        public string Date
        {
            get { return this.date; }
            set { this.date = value; OnPropertyChanged("Date"); }
        }


        public void Notify(int progress, Guid deviceId)
        {
            if (!deviceId.Equals(this.DeviceId))
                return;
            this.Progress = progress;
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// OnPropertyChanged method to raise the event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
