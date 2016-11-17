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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxWatches.ItemsSource = GeneActivDeviceService.GetInstance().ConnectedDevices;
            
            listBoxWatches.ItemTemplateSelector = new WatchTemplateSelector();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            SetupWindow w = new SetupWindow();
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

        // ---------------------------- GET WORKERS -------------------------------------
        List<BackgroundWorker> allBgWorkers = new List<BackgroundWorker>();

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Watch w = (Watch)e.Argument;
            BackgroundWorker worker = (BackgroundWorker)sender;

            for (int n = 0; n < w.Data; n++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }


                Thread.Sleep(1);
                w.Progress = 1 + (int)(((double)n) / w.Data * 100.0);
                worker.ReportProgress(1 + (int)(((double)n) / w.Data * 100.0));
            }

            w.Action = "Done";
            w.Data = 0;
            w.Name = w.Name + "";
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);

            //progressBar.Value = nr;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*     if (e.Cancelled)
                     label.Content = label.Content + " - Cancelled";
                 else
                     label.Content = label.Content + " - Done";*/
        }


        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;

            IGeneaDevice watch = ((ListViewDeviceItem)listBoxWatches.SelectedItem).Device;


            /*((Watch)listBoxWatches.SelectedItem).Action = "Reading";
            ((Watch)listBoxWatches.SelectedItem).Name = ((Watch)listBoxWatches.SelectedItem).Name + "";*/

            string resultFile = GeneActivDataConnector.GetInstance().DownloadData(watch);
            Debug.Write("DONEE" + resultFile);

            /*BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(listBoxWatches.SelectedItem);

            allBgWorkers.Add(worker);*/

        }

        // ---------------------------- SET WORKERS -------------------------------------
        List<BackgroundWorker> allBgWorkers2 = new List<BackgroundWorker>();

        void worker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Watch w = (Watch)e.Argument;
            BackgroundWorker worker = (BackgroundWorker)sender;

            for (int n = 0; n < 7000; n++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }


                Thread.Sleep(1);
                w.Progress = 1 + (int)(((double)n) / 7000 * 100.0);
                worker.ReportProgress(1 + (int)(((double)n) / 7000 * 100.0));
            }
            w.Action = "Done";
            w.Data = 0;
            w.Name = w.Name;
        }

        void worker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int nr = e.ProgressPercentage;
            Debug.WriteLine(nr);

            //progressBar.Value = nr;
        }

        void worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*     if (e.Cancelled)
                     label.Content = label.Content + " - Cancelled";
                 else
                     label.Content = label.Content + " - Done";*/
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxWatches.SelectedItem == null) return;
            ((Watch)listBoxWatches.SelectedItem).Action = "Writing";
            ((Watch)listBoxWatches.SelectedItem).Name = ((Watch)listBoxWatches.SelectedItem).Name + "";

            BackgroundWorker worker2 = new BackgroundWorker();
            worker2.DoWork += worker2_DoWork;
            worker2.ProgressChanged += worker2_ProgressChanged;
            worker2.RunWorkerCompleted += worker2_RunWorkerCompleted;

            worker2.WorkerSupportsCancellation = true;
            worker2.WorkerReportsProgress = true;
            worker2.RunWorkerAsync(listBoxWatches.SelectedItem);

            allBgWorkers2.Add(worker2);
        }
    }
}
