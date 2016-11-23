using System.Windows;
using System.Windows.Controls;

namespace IoTDataReceiver
{
    class MyClasses
    {

        public struct Settings
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
    }

}
