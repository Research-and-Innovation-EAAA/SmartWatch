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
    /// Author: Genea 
    /// </summary>
    public class ListViewDeviceItem : INotifyPropertyChanged
    {
        /// <summary>
        /// THE device - all settings, device I/O etc.
        /// </summary>
        public IGeneaDevice Device
        {
            get { return _device; }
            set { _device = value; OnPropertyChanged("Device"); }
        }
        private IGeneaDevice _device = null;

        /// <summary>
        /// Device battery level (%ge) from status updated
        /// </summary>
        public int BatteryLevel
        {
            get { Debug.Write("returning " +_batteryLevel + " %"); return _batteryLevel; }
        }
        private int _batteryLevel = 0;

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
