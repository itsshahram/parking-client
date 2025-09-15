using System.Net.NetworkInformation;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Threading.Timer;


namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseErrorWindow.xaml
    /// </summary>
    public partial class DatabaseErrorWindow : Window
    {
        private readonly string _dbHost;
        private bool _dbReachable = false;

        private Timer? _pingTimer;

        public DatabaseErrorWindow(string dbHost)
        {
            InitializeComponent();
            _dbHost = dbHost;

            _pingTimer = new Timer(async _ => await PingDatabase(), null, 0, 5000);
        }

        private async Task PingDatabase()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(_dbHost, 1000);

                if (reply.Status == IPStatus.Success)
                {
                    _dbReachable = true;
                    Dispatcher.Invoke(() =>
                    {
                        StatusBar.Value = 1;
                        StatusBar.Foreground = Brushes.Green;
                        TitleText.Text = "Database Connected!";
                        RetryButton.IsEnabled = true;
                    });
                }
                else
                {
                    _dbReachable = false;
                    Dispatcher.Invoke(() =>
                    {
                        StatusBar.Value = 1;
                        StatusBar.Foreground = Brushes.Red;
                        TitleText.Text = "Database unreachable.";
                        RetryButton.IsEnabled = false;
                    });
                }
            }
            catch
            {
                _dbReachable = false;
                Dispatcher.Invoke(() =>
                {
                    StatusBar.Value = 1;
                    StatusBar.Foreground = Brushes.Red;
                    TitleText.Text = "Database unreachable.";
                    RetryButton.IsEnabled = false;
                });
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dbReachable)
            {
                // Stop the ping timer
                _pingTimer?.Dispose();

                // Close this window and show the login
                var login = App.GetService<LoginWindow>();
                login?.Show();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Database is still unreachable. Please check network or server.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _pingTimer?.Dispose();
        }
    }
}
