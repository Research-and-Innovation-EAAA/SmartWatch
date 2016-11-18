using System.Windows;
using System.Windows.Controls;
using static Prototype.MyClasses;

namespace Prototype
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
            comboBox.ItemsSource = Service.Instance.Watches;
        }
    }
}
