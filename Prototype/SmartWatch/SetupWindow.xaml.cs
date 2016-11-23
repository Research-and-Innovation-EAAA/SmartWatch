using System.Windows;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        private IDataReceiver dataReceiver;
        private Settings settings;

        public SetupWindow(IDataReceiver dataReceiver)
        {
            InitializeComponent();
            this.dataReceiver = dataReceiver;
            this.settings = SettingsService.Instance.Settings;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.settings;
            this.tBoxUsername.DataContext = this;
        }

        public string Username { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Is this ok?\n" + SettingsService.Instance.Settings+"\nUsername: " + Username, "Ok?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SettingsService.Instance.Settings = this.settings; // TODO cancel + saving 
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
