using GeneActiv.GeneaLibrary;
using System;
using System.ComponentModel;
using System.Diagnostics;

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

        /// <summary>
        /// Device battery level (%ge) from status updated
        /// </summary>
        private int _batteryLevel = 0;
        public int BatteryLevel
        {
            get { Debug.Write("returning " +_batteryLevel + " %"); return _batteryLevel; }
        }
        
        /// <summary>
        /// Set current battery level based on raw battery voltage
        /// </summary>
        /// <param name="volts"></param>
        public void SetBatteryVoltage(float volts)
        {
            Debug.Write("setting voltage to " + volts + " V");
            _batteryLevel = GeneaUnitConvertor.CalculateBatteryLevel(volts);
            OnPropertyChanged("BatteryLevel");
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
