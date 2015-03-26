using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

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
            await Task.Run(() => Program.StartListeningForDevices());
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
