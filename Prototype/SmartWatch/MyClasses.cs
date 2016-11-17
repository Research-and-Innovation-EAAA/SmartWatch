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
            public int Frequency { get; set; }
            public int Period { get; set; }

            public string StudyCenter { get; set; }
            public string StudyCode { get; set; }

            public override string ToString()
            {
                return Frequency + " " + Period + " " + StudyCenter + " " + StudyCode;
            }
        }

        public class Service
        {
            private static Service service = null;

            List<Watch> watches = new List<Watch>();
            public List<Watch> Watches
            {
                get { return watches; }
            }

            public Watch FindWatch(string name)
            {
                foreach (Watch w in watches)
                {
                    if (w.Name == name)
                        return w;
                }
                return null;
            }

            Settings settings;
            public Settings Settings
            {
                get { return settings; }
            }

            private Service()
            {
                watches.Add(new Watch { Name = "Jakub", Data = 16345, Result = "Jakub sov godt." });
                //watches.Add(new Watch { Name = "Lisa", Data = 22843, Result = "Lisa sov dejligt." });
                watches.Add(new Watch { Name = "Søren", Data = 33400, Result = "Søren sov længe." });
                //   watches.Add(new Watch { Name = "Monica", Data = 25400, Result = "Monica sov ikke." });
                watches.Add(new Watch { Name = "Simon", Data = 0, Result = "No data." });

                settings = new Settings { Frequency = 75, Period = 7, StudyCenter = "AUH", StudyCode = "Gigt" };
            }



            public static Service Instance
            {
                get
                {
                    if (service == null)
                        service = new Service();

                    return service;
                }
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

    public class ResultsConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            Watch w = Service.Instance.FindWatch(name);
            if (w.Data == 0)
                return w.Result;
            return "No data loaded.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressBarConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            Watch w = Service.Instance.FindWatch(name);
            if (w.Action == "Reading")
                return new SolidColorBrush(Colors.LightGreen);
            if (w.Action == "Done")
                return new SolidColorBrush(Colors.Gray);
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
