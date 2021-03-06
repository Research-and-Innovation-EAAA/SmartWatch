﻿using System;
using System.ComponentModel;

namespace IoTDataReceiver
{

    /// <summary>
    /// Class for representing a device.
    /// </summary>
    public class DeviceInformation : INotifyPropertyChanged
    {
        /// <summary>
        /// Device Guid, generated everytime a device is connected
        /// </summary>
        private Guid deviceId;
        public Guid DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; OnPropertyChanged("DeviceId"); }
        }

        /// <summary>
        /// Physical number on the device
        /// </summary>
        private string deviceNumber;
        public string DeviceNumber
        {
            get { return deviceNumber; }
            set { deviceNumber = value; OnPropertyChanged("DeviceNumber"); }
        }

        private string patientName;
        public string PatientName
        {
            get { return patientName; }
            set { patientName = value; OnPropertyChanged("PatientName"); }
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// INotifyPropertyChanged - required to allow control to change automatically with data
        /// and vice-versa
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
