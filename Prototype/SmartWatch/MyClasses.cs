using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static IoTDataReceiver.DataReceiver;

namespace IoTDataReceiver
{
    public class MyClasses
    {

        public delegate void DeviceProgressUpdateHandler(int percent, Guid deviceId);
        public delegate void ProgressUpdateHandler(int percent);

        /*    public struct Settings
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

        public class DeviceTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                DeviceData device = ((DeviceData)item);

                Window window = Application.Current.MainWindow;

                if (device.Connected)
                {
                    return (DataTemplate)window.FindResource("ConnectedDeviceTemplate");
                }
                else
                {
                    return (DataTemplate)window.FindResource("DisconnectedDeviceTemplate");
                }
            }
        }



        public class MyBackgroundWorker : BackgroundWorker
        {

        }
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
