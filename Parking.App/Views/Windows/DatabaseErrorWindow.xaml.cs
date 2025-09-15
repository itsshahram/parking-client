using Brushes = System.Windows.Media.Brushes;
using Timer = System.Threading.Timer;

namespace Parking.App.Views.Windows
{
    public partial class DatabaseErrorWindow : Window
    {
        private readonly string _dbHost;
        private bool _dbReachable = false;

        private Timer? _pingTimer;

        public DatabaseErrorWindow(string dbHost)
        {
            InitializeComponent();
            _dbHost = dbHost;

            _pingTimer = new Timer(async _ => await CheckDatabaseAvailability(), null, 0, 5000);
        }

        private async Task CheckDatabaseAvailability()
        {
            try
            {
                using (var context = App.GetService<ApplicationDbContext>())
                {
                    if (await context.Database.CanConnectAsync())
                    {
                        _dbReachable = true;
                        Dispatcher.Invoke(() =>
                        {
                            StatusBar.Value = 1;
                            StatusBar.Foreground = Brushes.Green;
                            TitleText.Text = "اتصال به پایگاه داده برقرار شد ✅";
                            RetryButton.IsEnabled = true;
                        });
                        return;
                    }
                }

                _dbReachable = false;
                Dispatcher.Invoke(() =>
                {
                    StatusBar.Value = 1;
                    StatusBar.Foreground = Brushes.Red;
                    TitleText.Text = "ارتباط با پایگاه داده برقرار نشد";
                    RetryButton.IsEnabled = false;
                });
            }
            catch
            {
                _dbReachable = false;
                Dispatcher.Invoke(() =>
                {
                    StatusBar.Value = 1;
                    StatusBar.Foreground = Brushes.Red;
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
