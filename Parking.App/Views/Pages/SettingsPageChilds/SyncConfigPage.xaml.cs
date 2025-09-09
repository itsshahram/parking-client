namespace Parking.App.Views.Pages.SettingsPageChilds
{
    public partial class SyncConfigPage : Page
    {
        private readonly ILogger<SyncConfigPage> _logger;
        private readonly ISynchronizationService _synchronizationService;


        public SyncConfigPage()
        {
            _logger = App.GetService<ILogger<SyncConfigPage>>();
            _synchronizationService = App.GetService<ISynchronizationService>();
            InitializeComponent();
        }

        private async void SetUserToken()
        {
            if (Settings.Default.Application_Sync_Enable is false)
            {
                if (string.IsNullOrEmpty(TokenStore.BearerToken))
                {
                    var credentials = LoadCredentials();

                    string username = string.Empty;
                    string password = string.Empty;

                    if (credentials == null)
                    {
                        var loginWindow = new TempLoginWindows
                        {
                            Owner = Application.Current.MainWindow
                        };

                        if (loginWindow.ShowDialog() != true)
                            return;

                        username = loginWindow.Username;
                        password = loginWindow.Password;
                    }
                    else
                    {
                        username = credentials.Value.Username;
                        password = credentials.Value.Password;

                        var loginResult = await _synchronizationService.CheckTokenAsync(username, password);

                        if (!loginResult.Succeeded)
                        {
                            var loginWindow = new TempLoginWindows
                            {
                                Owner = Application.Current.MainWindow
                            };

                            if (loginWindow.ShowDialog() != true)
                                return;

                            username = loginWindow.Username;
                            password = loginWindow.Password;
                        }
                    }

                    _logger.LogInformation("Token is now set in TokenStore.");
                }
            }
        }




        public static (string Username, string Password)? LoadCredentials()
        {
            if (!File.Exists(Constants.CredentialsPath))
                return null;

            byte[] key = GetKey();
            byte[] encrypted = File.ReadAllBytes(Constants.CredentialsPath);
            string decrypted = AesEncryption.Decrypt(encrypted, key);

            string[] parts = decrypted.Split(':');
            if (parts.Length == 2)
                return (parts[0].Trim(), parts[1].Trim());

            return null;
        }

        private async void SyncUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetUserToken();
                ShowProgressRing(true);
                await _synchronizationService.GetParkingLotAccountsFromServerAsync();
                ShowProgressRing(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncUsers_Click");
            }
        }

        private async void SyncPriceList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetUserToken();
                ShowProgressRing(true);
                await _synchronizationService.ReceiveVehicleSegmentsListFromServerAsync();
                ShowProgressRing(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncUsers_Click");
            }
        }

        private async void SyncLicensePlateGroupList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetUserToken();
                ShowProgressRing(true);
                await _synchronizationService.ReceiveLicensePlateGroupFromServerAsync();
                ShowProgressRing(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncUsers_Click");
            }
        }
        private async void ShowProgressRing(bool show)
        {
            if (show)
                this.Dispatcher.Invoke(() =>
                {
                    ProgressRing.Visibility = Visibility.Visible;
                    ProgressRing.IsIndeterminate = true;
                    SyncLicensePlateGroupList.IsEnabled = false;
                    SyncPriceList.IsEnabled = false;
                    SyncUsers.IsEnabled = false;
                });
            else
                this.Dispatcher.Invoke(() =>
                {
                    ProgressRing.Visibility = Visibility.Collapsed;
                    ProgressRing.IsIndeterminate = false;
                    SyncLicensePlateGroupList.IsEnabled = true;
                    SyncPriceList.IsEnabled = true;
                    SyncUsers.IsEnabled = true;
                });

        }
        private static byte[] GetKey()
            => AesEncryption.LoadKey(Constants.KeyPath);
    }
}
