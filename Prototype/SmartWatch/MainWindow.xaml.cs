﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IProgressObserver
    {
        private IDataReceiver dataReceiver;
        public MainWindow()
        {
            InitializeComponent();
            this.dataReceiver = DataReceiver.Instance;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxWatches.ItemsSource = dataReceiver.GetConnectedDevices();
            listBoxWatches.ItemTemplateSelector = new WatchTemplateSelector();

            BindingOperations.SetBinding(btnGet, Button.IsEnabledProperty, new Binding() //TODO reuse the binding
            {
                Source = this.dataReceiver,
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.WatchInserted // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnProcess, Button.IsEnabledProperty, new Binding() //TODO reuse the binding
            {
                Source = this.dataReceiver,
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataDownloaded // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnUpload, Button.IsEnabledProperty, new Binding() //TODO reuse the binding
            {
                Source = this.dataReceiver,
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataProcessed // <-- when to be enabled
            });

            BindingOperations.SetBinding(btnClear, Button.IsEnabledProperty, new Binding() //TODO reuse the binding
            {
                Source = this.dataReceiver,
                Path = new PropertyPath("CurrentStep"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new EnabledStepConverter(),
                ConverterParameter = DataProcessStep.DataUploaded // <-- when to be enabled
            });
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            SetupWindow w = new SetupWindow(dataReceiver);
            w.ShowDialog();
        }

        private void btnShowData_Click(object sender, RoutedEventArgs e)
        {
            DataWindow w = new DataWindow();
            w.ShowDialog();
        }

        private void listBoxWatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            watchPanel.DataContext = listBoxWatches.SelectedItem;
        }

        // --------------------------------------- GET ---------------------------------------
        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;

            Guid deviceId = ((ListViewDeviceItem)listBoxWatches.SelectedItem).DeviceId;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerGet_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerGet_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(deviceId);
        }


        void workerGet_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid deviceId = (Guid)e.Argument;

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            try
            {
                dataReceiver.GetData(deviceId);
            }
            catch (MyExceptions.NoDataException ex)
            {
                this.worker.ReportProgress(0);
                MessageBox.Show("This smart watch does not contain any data.\n" + ex.Message);
            }
            catch (MyExceptions.CommunicationException ex)
            {
                this.worker.ReportProgress(0);
                MessageBox.Show("Error reading data.\n" + ex.Message);
            }
            finally
            {
                ((ProgressSubject)dataReceiver).UnregisterObserver(this);
                this.worker = null;
            }
            Debug.Write("DONE");


        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);
            progressBar.IsIndeterminate = nr == -1;
            progressBar.Value = nr;
        }

        void workerGet_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*     if (e.Cancelled)
                     label.Content = label.Content + " - Cancelled";
                 else
                     label.Content = label.Content + " - Done";*/
            Debug.Write("DONEE");
        }


        // --------------------------------------- PROCESS ---------------------------------------
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerProcess_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerProcess_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }


        void workerProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            //   Guid deviceId = (Guid)e.Argument;

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            dataReceiver.ProcessData();
            Debug.Write("DONE");

            ((ProgressSubject)dataReceiver).UnregisterObserver(this);
            this.worker = null;
        }

        void workerProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }


        // --------------------------------------- UPLOAD ---------------------------------------
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerSend_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerSend_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }


        void workerSend_DoWork(object sender, DoWorkEventArgs e)
        {
            //   Guid deviceId = (Guid)e.Argument;

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            try
            {
                dataReceiver.SendData();
            }
            catch (MyExceptions.UnauthorizedException ex)
            {
                this.worker.ReportProgress(0);
                MessageBox.Show("Wrong password for the patient, cannot log in.\n" + ex.Message);
            }
            finally
            {
                ((ProgressSubject)dataReceiver).UnregisterObserver(this);
                this.worker = null;
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
            if (listBoxWatches.SelectedItem == null) return;

            Guid deviceId = ((ListViewDeviceItem)listBoxWatches.SelectedItem).DeviceId;

            SetupWindow w = new SetupWindow(dataReceiver);
            w.Owner = this;
            w.ShowDialog();

            string username = w.Username;

            object[] args = new object[] { deviceId, username };

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerClear_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += workerClear_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(args);

        }

        void workerClear_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            Guid deviceId = (Guid)args[0];
            string username = (string)args[1];

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            try
            {
                dataReceiver.PrepareDevice(deviceId, username, SettingsService.Instance.Settings);
            }
            catch (MyExceptions.DeviceException ex)
            {
                this.worker.ReportProgress(0);
                MessageBox.Show("Error setting up the device.\n" + ex.Message);
            }
            finally
            {
                ((ProgressSubject)dataReceiver).UnregisterObserver(this);
                this.worker = null;
            }


            Debug.Write("DONE");

        }

        void workerClear_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }


        private BackgroundWorker worker = null;

        public void Notify(int progress)
        {
            worker.ReportProgress(progress);
        }
    }
}
