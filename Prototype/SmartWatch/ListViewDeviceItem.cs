using GeneActiv.GeneaLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{

    /// <summary>
    /// Container for list view item representing a device plus app-specific state/control
    /// backing. Need INotifyPropertyChanged so we can update control from code-behind
    /// Author: Activinsights Ltd., edited
    /// </summary>
    public class ListViewDeviceItem : INotifyPropertyChanged
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
        /// Device number - physical number of the device
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
