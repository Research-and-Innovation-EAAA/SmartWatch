using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static IoTDataReceiver.MyExceptions;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IService dataReceiver;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                this.dataReceiver = Service.Instance;
            }
            catch (MyExceptions.InputException e)
            {
                MessageBox.Show("The file with patients' passwords is incorrectly formatted, and therefore cannot be read properly.\n\nThere must be one patient per line without spaces: username,password.\n\nTechnical info: " + e.Message, "Error in patients.csv", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(0);
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxDevices.ItemsSource = dataReceiver.GetAvailableDevices();

            BindingOperations.SetBinding(btnGet, Button.IsEnabledProperty, new Binding()
            {
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = false,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DeviceInserted // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnProcess, Button.IsEnabledProperty, new Binding() // cannot reuse the existing binding, needs to be set up again
            {
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = false,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataDownloaded // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnUpload, Button.IsEnabledProperty, new Binding()
            {
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = false,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataProcessed // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnClear, Button.IsEnabledProperty, new Binding()
            {
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = false,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataUploaded // <-- when to be enabled
            });
        }



        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            SetupWindow w = new SetupWindow();
            w.ShowDialog();
        }

        private void listBoxWatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            devicePanel.DataContext = listBoxDevices.SelectedItem;
        }

        // --------------------------------------- GET ---------------------------------------
        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDevices.SelectedItem == null) return;

            Guid deviceId = ((DeviceData)listBoxDevices.SelectedItem).DeviceId;
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerGet_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerGet_RunWorkerCompleted;


            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = false;
            worker.RunWorkerAsync(deviceId);
        }


        void workerGet_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid deviceId = (Guid)e.Argument;

            try
            {
                dataReceiver.GetData(deviceId);
            }
            catch (MyExceptions.NoDataException ex)
            {
                MessageBox.Show("This smart watch does not contain any data.\n" + ex.Message);
            }
            catch (MyExceptions.CommunicationException ex)
            {
                MessageBox.Show("Error reading data.\n" + ex.Message);
            }
            finally
            {
            }
            Debug.Write("DONE");


        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            /*         int nr = e.ProgressPercentage;
                     Debug.WriteLine(nr);
                     progressBar.IsIndeterminate = nr == -1;
                     progressBar.Value = nr;*/
        }

        void workerGet_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }


        // --------------------------------------- PROCESS ---------------------------------------
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDevices.SelectedItem == null) return;

            Guid deviceId = ((DeviceData)listBoxDevices.SelectedItem).DeviceId;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerProcess_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerProcess_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = false;
            worker.RunWorkerAsync(deviceId);
        }


        void workerProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid deviceId = (Guid)e.Argument;

            try
            {
                dataReceiver.ProcessData(deviceId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing data.\n" + ex.Message);
            }
            Debug.Write("DONE");

        }

        void workerProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }


        // --------------------------------------- UPLOAD ---------------------------------------
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDevices.SelectedItem == null) return;

            Guid deviceId = ((DeviceData)listBoxDevices.SelectedItem).DeviceId;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerSend_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerSend_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = false;
            worker.RunWorkerAsync(deviceId);
        }


        void workerSend_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid deviceId = (Guid)e.Argument;

            try
            {
                dataReceiver.SendData(deviceId);
            }
            catch (MyExceptions.UnauthorizedException ex)
            {
                MessageBox.Show("Wrong password for the patient, cannot log in.\n" + ex.Message);
            }
            catch (MyExceptions.CommunicationException ex)
            {
                MessageBox.Show("Error communication with server.\n" + ex.Message);
            }
            catch (MyExceptions.UnknownPatientException ex)
            {
                MessageBox.Show("Unknown patient, cannot find information.\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending data.\n" + ex.Message);
            }

            Debug.Write("DONE");
        }

        void workerSend_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }


        // --------------------------------------- CLEAR&SETUP ---------------------------------------
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDevices.SelectedItem == null) return;

            DeviceData device = (DeviceData)listBoxDevices.SelectedItem;
            if (!device.Connected) return;

            Guid deviceId = device.DeviceId;

            SetupWindow w = new SetupWindow();
            w.Owner = this;
            w.ShowDialog();

            if (!w.EraseAndSetup) // if the user cancelled, terminate
                return;

            string username = w.Username;

            object[] args = new object[] { deviceId, username };

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerClear_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerClear_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = false;
            worker.RunWorkerAsync(args);

        }

        void workerClear_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            Guid deviceId = (Guid)args[0];
            string username = (string)args[1];

            try
            {
                dataReceiver.PrepareDevice(deviceId, username);
            }
            catch (MyExceptions.DeviceException ex)
            {
                MessageBox.Show("Error setting up the device.\n" + ex.Message);
            }
        }

        void workerClear_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Setting up watch done!");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
            if (MessageBox.Show("Do you want really to exit the app?", "Exit?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
