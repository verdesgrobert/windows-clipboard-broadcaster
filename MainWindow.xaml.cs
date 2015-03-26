using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Appboxstudios.ClipboardBroadcaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string _status;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            ClipBoardHelper.SetOutput(msg =>
            {
                Status = msg;
            });
            DataContext = this;
        }

        public static readonly DependencyProperty RemoteAddressesProperty = DependencyProperty.Register(
            "RemoteAddresses", typeof(ObservableCollection<MyIpAddress>), typeof(MainWindow), new PropertyMetadata(default(ObservableCollection<MyIpAddress>)));

        public ObservableCollection<MyIpAddress> RemoteAddresses
        {
            get { return (ObservableCollection<MyIpAddress>)GetValue(RemoteAddressesProperty); }
            set { SetValue(RemoteAddressesProperty, value); }
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var th = new Thread(async () =>
            {
                await Task.Run(() => ClipBoardHelper.StartListeningForDevices(RemoteAddresses.Add));
            });
            th.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
