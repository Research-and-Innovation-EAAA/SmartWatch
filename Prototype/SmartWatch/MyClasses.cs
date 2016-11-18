using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    class MyClasses
    {
        public class Watch : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected void Notify(string propName)
            {
                if (this.PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
                }
            }

            string name;
            public string Name
            {
                get { return name; }
                set
                {
                    /*  if (this.name == value) { return; }*/
                    this.name = value;

                    // OBS!!
                    Notify("Name");
                }
            }

            int data;
            public int Data
            {
                get { return data; }
                set
                {
                    if (this.data == value) { return; }
                    this.data = value;

                    // OBS!!
                    Notify("Data");
                }
            }

            int progress;
            public int Progress
            {
                get { return progress; }
                set
                {
                    if (this.progress == value) { return; }
                    this.progress = value;

                    // OBS!!
                    Notify("Progress");
                }
            }

            string action;
            public string Action
            {
                get { return action; }
                set
                {
                    if (this.action == value) { return; }
                    this.action = value;

                    // OBS!!
                    Notify("Action");
                }
            }

            string result;

            public string Result
            {
                get { return result; }
                set
                {
                    if (this.result == value) { return; }
                    this.result = value;

                    // OBS!!
                    Notify("Result");
                }
            }


            public override string ToString()
            {
                return name + " " + data;
            }
        }

        public class Settings
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
