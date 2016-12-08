using System;
using System.Globalization;
using System.Windows.Data;

namespace IoTDataReceiver
{
    public class MyClasses
    {

        public delegate void DeviceProgressUpdateHandler(int percent, Guid deviceId);

        /*  struct Settins was replaced by generic Dictionary<string, string>
         *  
         *    public struct Settings
            {
                public float Frequency { get; set; }
                public int Period { get; set; }

                public string StudyCenter { get; set; }
                public string StudyCode { get; set; }

                public override string ToString()
                {
                    return Frequency + " " + Period + " " + StudyCenter + " " + StudyCode;
                }
            }
          */
    }

    public class EnabledStepConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataProcessStep && parameter != null && parameter is DataProcessStep)
            {
                if ((DataProcessStep)parameter == DataProcessStep.DataUploaded)
                    return true; // allow setting up at all times

                return (DataProcessStep)value == (DataProcessStep)parameter;
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndeterminateProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            return val == -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
