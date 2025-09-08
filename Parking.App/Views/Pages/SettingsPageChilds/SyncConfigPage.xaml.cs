using Microsoft.IdentityModel.Tokens;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for AccountsSettingsPage.xaml
    /// </summary>
    public partial class SyncConfigPage : Page
    {
        private readonly ILogger<SyncConfigPage> _logger;
        private readonly ISynchronizationService _synchronizationService;
        private static readonly string AppDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parking.App");
        private static readonly string CredentialsPath = Path.Combine(AppDataFolder, "credentials.dat");
        private static readonly string KeyPath = Path.Combine(AppDataFolder, "aeskey.bin");

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
                    var creds = LoadCredentials();

                    string username, password;

                    if (creds == null)
                    {
                        var loginWindow = new TempLoginWindows
                        {
                            Owner = Application.Current.MainWindow
                        };

                        if (loginWindow.ShowDialog() == true)
                        {
                            username = loginWindow.Username;
                            password = loginWindow.Password;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        username = creds.Value.Username;
                        password = creds.Value.Password;
                    }

                    var loginToServerResult = await _synchronizationService?.CheckTokenAsync(username, password);

                    if (loginToServerResult.Succeeded)
                    {
                        _logger.LogInformation("New bearer token retrieved.");
                    }
                    else
                    {
                        _logger.LogInformation("");
                    }
                }
            }
        }


        public static (string Username, string Password)? LoadCredentials()
        {
            if (!File.Exists(CredentialsPath))
                return null;

            byte[] key = GetOrCreateKey();
            byte[] encrypted = File.ReadAllBytes(CredentialsPath);
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
        private static byte[] GetOrCreateKey()
            => AesEncryption.LoadKey(KeyPath);
    }
}
