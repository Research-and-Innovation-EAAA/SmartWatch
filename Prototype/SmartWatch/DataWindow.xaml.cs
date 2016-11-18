﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static IoTDataReceiver.MyClasses;

namespace IoTDataReceiver
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataWindow()
        {
            InitializeComponent();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            groupBox.DataContext = comboBox.SelectedItem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         //   comboBox.ItemsSource = Service.Instance.Watches;
        }
    }
}
