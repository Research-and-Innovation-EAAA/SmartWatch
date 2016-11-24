using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace IoTDataReceiver
{
    public class MyClasses
    {

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

        public class WatchTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                ListViewDeviceItem watch = ((ListViewDeviceItem)item);

                Window window = Application.Current.MainWindow;

                /*   if (watch.Data > 0)
                   {*/
                return (DataTemplate)window.FindResource("FullDataTemplate");
                /*  }
                  else
                  {
                      return (DataTemplate)window.FindResource("EmptyDataTemplate");
                  }*/
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

                    return (DataProcessStep)value == (DataProcessStep) parameter;
                }
                else return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
