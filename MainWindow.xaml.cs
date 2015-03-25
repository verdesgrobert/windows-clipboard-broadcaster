using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Appboxstudios.ClipboardBroadcaster;

namespace Appboxstudios.ClipboardBroadcaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _status;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            Program.SetOutput(msg =>
            {
                Status = msg;
            });
            DataContext = this;
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Program.Main(null));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
