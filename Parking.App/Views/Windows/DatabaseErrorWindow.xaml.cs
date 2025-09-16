using Timer = System.Threading.Timer;

namespace Parking.App.Views.Windows
{
    public partial class DatabaseErrorWindow : FluentWindow
    {
        private readonly string _dbHost;
        private bool _dbReachable = false;

        private readonly BitmapImage _iconConnected =
            new BitmapImage(new Uri("pack://application:,,,/Assets/connection.png"));

        private readonly BitmapImage _iconDisconnected =
            new BitmapImage(new Uri("pack://application:,,,/Assets/no-internet.png"));

        private readonly BitmapImage _iconLoading =
            new BitmapImage(new Uri("pack://application:,,,/Assets/loading-bar.png"));


        private Timer? _pingTimer;

        public DatabaseErrorWindow(string dbHost)
        {
            InitializeComponent();
            _dbHost = dbHost;

            // Start background check every 5s
            _pingTimer = new Timer(async _ => await CheckDatabaseAvailability(), null, 0, 5000);
        }

        private async Task CheckDatabaseAvailability()
        {
            try
            {
                using (var context = App.GetService<ApplicationDbContext>())
                {
                    var connectTask = context.Database.CanConnectAsync();
                    var timeoutTask = Task.Delay(3000);

                    if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
                    {
                        if (await connectTask)
                        {
                            _dbReachable = true;
                            Dispatcher.Invoke(() =>
                            {
                                StatusImage.Source = _iconConnected;
                                TitleText.Text = "اتصال به پایگاه داده برقرار شد ✅";
                                RetryButton.IsEnabled = true;
                            });
                            return;
                        }
                    }
                }

                _dbReachable = false;
                Dispatcher.Invoke(() =>
                {
                    StatusImage.Source = _iconDisconnected;
                    TitleText.Text = "ارتباط با پایگاه داده برقرار نشد";
                    RetryButton.IsEnabled = false;
                });
            }
            catch
            {
                _dbReachable = false;
                Dispatcher.Invoke(() =>
                {
                    StatusImage.Source = _iconDisconnected;
                    TitleText.Text = "ارتباط با پایگاه داده برقرار نشد";
                    RetryButton.IsEnabled = false;
                });
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dbReachable)
            {
                _pingTimer?.Dispose();
                var login = App.GetService<LoginWindow>();
                login?.Show();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("پایگاه داده هنوز در دسترس نیست. لطفاً شبکه یا سرور را بررسی کنید.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _pingTimer?.Dispose();
        }
    }
}