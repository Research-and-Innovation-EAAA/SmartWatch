using System.Collections.Generic;
using System.Windows;
using static IoTDataReceiver.MyExceptions;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        private IPatientService patientService = null;
        private ISettingsService settingsService = null;
        private List<string> frequencies;
        public Dictionary<string, string> Settings { get; set; }
        public string Username { get; set; }
        public bool EraseAndSetup { get; set; }

        public SetupWindow()
        {
            InitializeComponent();

            this.patientService = PatientService.Instance;
            this.settingsService = SettingsService.Instance;

            this.Settings = new Dictionary<string, string>(settingsService.Settings); // copy of existing settings, to allow rolling back (cancel button)
            this.frequencies = new List<string> { "10", "20", "25", "30", "40", "50", "60", "66,7", "75", "85,7", "100" };
            this.EraseAndSetup = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            cBoxPatients.ItemsSource = patientService.GetPatients();
            cBoxFrequency.ItemsSource = this.frequencies;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Username))
            {
                MessageBox.Show("Please choose a patient.", "Patient missing");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Do you really want to erase all the data from the smart watch?", "Erase?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                settingsService.Settings = this.Settings;
                this.EraseAndSetup = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
