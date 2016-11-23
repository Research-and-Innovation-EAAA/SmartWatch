using System.Collections.Generic;
using System.Windows;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        //private Dictionary<string, string> settings;
        public Dictionary<string, string> Settings { get; set; }

        public SetupWindow(IDataReceiver dataReceiver)
        {
            InitializeComponent();
            this.Settings = new Dictionary<string, string>(SettingsService.Instance.Settings); // copy of existing settings, to allow rolling back (cancel button)
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            cBoxPatients.ItemsSource = PatientService.Instance.GetPatients();
        }

        public string Username { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Is this ok?\n" + SettingsService.Instance.Settings+"\nUsername: " + Username, "Ok?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SettingsService.Instance.Settings = this.Settings; // TODO cancel + saving 
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
