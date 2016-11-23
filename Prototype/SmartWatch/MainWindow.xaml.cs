using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeneActiv.GeneaLibrary;
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
            this.dataReceiver = new DataReceiver();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxWatches.ItemsSource = dataReceiver.GetConnectedDevices();

            listBoxWatches.ItemTemplateSelector = new WatchTemplateSelector();
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

        //     List<BackgroundWorker> allBgWorkers = new List<BackgroundWorker>();

        // --------------------------------------- GET ---------------------------------------
        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;

            Guid deviceId = ((ListViewDeviceItem)listBoxWatches.SelectedItem).DeviceId;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerGet_DoWork;
            worker.ProgressChanged += workerGet_ProgressChanged;
            worker.RunWorkerCompleted += workerGet_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(deviceId);

            //            allBgWorkers.Add(worker);

        }



        void workerGet_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid deviceId = (Guid)e.Argument;

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            dataReceiver.GetData(deviceId);
            Debug.Write("DONE");

            ((ProgressSubject)dataReceiver).UnregisterObserver(this);
            this.worker = null;
        }

        void workerGet_ProgressChanged(object sender, ProgressChangedEventArgs e)
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
            worker.ProgressChanged += workerProcess_ProgressChanged;
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

        void workerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);
            progressBar.IsIndeterminate = nr == -1;
            progressBar.Value = nr;
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
            worker.ProgressChanged += workerSend_ProgressChanged;
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

            dataReceiver.SendData();
            Debug.Write("DONE");

            ((ProgressSubject)dataReceiver).UnregisterObserver(this);
            this.worker = null;
        }

        void workerSend_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);
            progressBar.IsIndeterminate = nr == -1;
            progressBar.Value = nr;
        }

        void workerSend_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write("DONEE");
        }

        // --------------------------------------- CLEAR&SETUP ---------------------------------------
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;


            SetupWindow w = new SetupWindow(dataReceiver);
            w.Owner = this;
            w.ShowDialog();



        /*    BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workerSet_DoWork;
            worker.ProgressChanged += workerSet_ProgressChanged;
            worker.RunWorkerCompleted += workerSet_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = false; // TODO cancel???
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();*/

        }

        void workerSet_DoWork(object sender, DoWorkEventArgs e)
        {
            //   Guid deviceId = (Guid)e.Argument;

            ((ProgressSubject)dataReceiver).RegisterObserver(this);
            this.worker = (BackgroundWorker)sender;

            dataReceiver.SendData();
            Debug.Write("DONE");

            ((ProgressSubject)dataReceiver).UnregisterObserver(this);
            this.worker = null;
        }

        void workerSet_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);
            progressBar.IsIndeterminate = nr == -1;
            progressBar.Value = nr;
        }

        void workerSet_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
