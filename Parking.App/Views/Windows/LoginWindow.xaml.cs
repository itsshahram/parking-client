using Parking.App.Models.GeneralServiceResponse;
using Parking.Domain.Entities.User;

using static Parking.App.Helpers.InternetChecker;


namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : FluentWindow
    {
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly IUserService? _userService;
        private readonly ISynchronizationService? _synchronizationService;
        private readonly IParkingService? _parkingService;
        private readonly ILogger<LoginWindow> _logger;

        private static string CredenatialsPath = AppDomain.CurrentDomain.BaseDirectory + "_encryptionKey.dat";
        private static readonly byte[] CrendentialsKey = Encoding.UTF8.GetBytes("1234567890123456");
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("1234567890123456");

        public LoginWindow()
        {
            InitializeComponent();
            _userManager = App.GetService<UserManager<ApplicationUser>>();
            _synchronizationService = App.GetService<ISynchronizationService>();
            _logger = App.GetService<ILogger<LoginWindow>>();
            _userService = App.GetService<IUserService>();
            _parkingService = App.GetService<IParkingService>();
            ContentRendered += LoginWindow_ContentRendered;
        }

        private  void LoginWindow_ContentRendered(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var creds = LoadCredentials();

                if (creds != null)
                {
                    usernameBox.Text = creds.Value.Username;
                    passwordBox.Password = creds.Value.Password;

                    Login();
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginBtn_Click");
            }
        }

        private void usernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (usernameBox.Text != null && usernameBox.Text.Length > 3 && passwordBox.Text != null && passwordBox.Text.Length > 2)
                LoginBtn.IsEnabled = true;
            else LoginBtn.IsEnabled = false;
        }

        private void passwordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (usernameBox.Text != null && usernameBox.Text.Length > 3 && passwordBox.Text != null && passwordBox.Text.Length > 2)
                LoginBtn.IsEnabled = true;
            else LoginBtn.IsEnabled = false;
        }

        private async void LoginWindow_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Login();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginWindow_KeyUp");
                Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                ms.Title = "خطا";
                ms.Content = "خطا در دریافت اطلاعات ورود";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                await ms.ShowDialogAsync();
            }


        }
        private async void Login()
        {

            ExitBtn.IsEnabled = false;
            LoginBtn.IsEnabled = false;
            LoginProgressBar.Visibility = Visibility.Visible;
            bool rememberMe = chkRemember.IsChecked.Value;

            if (CheckUsers())
            {
                if (usernameBox.Text != null && usernameBox.Text.Length > 3 && passwordBox.Text != null && passwordBox.Text.Length > 2)
                {
                    if (!IsInternetAvailable())
                    {
                        Settings.Default.Application_Sync_Enable = false;
                        Settings.Default.Save();
                    }

                    var username = usernameBox.Text;
                    var pasword = passwordBox.Password;
                    var result = _userService?.Login(username, pasword);


                    if (result == Domain.General.LoginStatus.NotActice)
                    {
                        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                        ms.Title = "خطا";
                        ms.Content = "کاربر فعال نمیباشد";
                        ms.IsPrimaryButtonEnabled = false;
                        ms.IsSecondaryButtonEnabled = false;
                        ms.CloseButtonText = "متوجه شدم";
                        await ms.ShowDialogAsync();
                    }
                    bool syncStatus = false;
                    if (Settings.Default.Application_Sync_Enable)
                    {

                        var loginToServerResult = await _synchronizationService?.CheckTokenAsync(username, pasword);
                        if (loginToServerResult.Succeeded)
                        {
                            syncStatus = loginToServerResult.Succeeded;
                        }
                    }
                    else
                        syncStatus = true;


                    if (result == Domain.General.LoginStatus.Success && syncStatus)
                    {
                        if (rememberMe is true)
                            SaveCredentails(username, pasword);

                        var user = _userService.GetUserByUsername(username);
                        var parking = _parkingService.GetParkingLotDetails();
                        if (parking.Succeeded)
                        {
                            TokenStore.ParkingLotId = parking.Result.Id;
                        }
                        TokenStore.FullName = user.Firstname + " " + user.Lastname;
                        TokenStore.Username = username;
                        TokenStore.RoleName = _userService.GetUserRoleByUserId(user.Id);
                        TokenStore.UserId = user.Id;
                        var mainWindow = App.GetService<MainWindow>();
                        Application.Current.MainWindow = mainWindow;
                        SingleInstanceApp.SetMainWindow(mainWindow ?? new MainWindow());

                        mainWindow?.Show();
                        this.Close();
                    }
                    else
                    {
                        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                        ms.Title = "خطا";
                        ms.Content = "نام کاربری و یا رمز عبور اشتباه است";
                        ms.IsPrimaryButtonEnabled = false;
                        ms.IsSecondaryButtonEnabled = false;
                        ms.CloseButtonText = "متوجه شدم";
                        await ms.ShowDialogAsync();
                    }
                }
            }
            else
            {
                if (usernameBox.Text != null && usernameBox.Text.Length > 3 && passwordBox.Text != null && passwordBox.Text.Length > 2)
                {
                    var username = usernameBox.Text;
                    var pasword = passwordBox.Password;
                    var result = await _synchronizationService?.CheckTokenAsync(username, pasword);
                    if (result.Succeeded)
                    {

                        if (rememberMe is true)
                            SaveCredentails(username, pasword);

                        var syncResult = await StartSyncJobs();

                        if (syncResult)
                        {
                            var mainWindow = App.GetService<MainWindow>();
                            Application.Current.MainWindow = mainWindow;
                            SingleInstanceApp.SetMainWindow(mainWindow ?? new MainWindow());
                            mainWindow?.Show();

                            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                            ms.Title = "موفق";
                            ms.Content = "همگام سازی اطلاعات با موفقیت انجام شد، لطفا اپلیکیشن را مجددا راه اندازی کنید";
                            ms.IsPrimaryButtonEnabled = false;
                            ms.IsSecondaryButtonEnabled = false;
                            ms.CloseButtonText = "متوجه شدم";
                            await ms.ShowDialogAsync();

                            this.Close();
                        }
                        else
                        {
                            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                            ms.Title = "خطا";
                            ms.Content = "خطا در دریافت اطلاعات از سرور";
                            ms.IsPrimaryButtonEnabled = false;
                            ms.IsSecondaryButtonEnabled = false;
                            ms.CloseButtonText = "متوجه شدم";
                            await ms.ShowDialogAsync();
                        }

                    }
                    else
                    {
                        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                        ms.Title = "خطا";
                        ms.Content = "نام کاربری و رمز عبور اشتباه است";
                        ms.IsPrimaryButtonEnabled = false;
                        ms.IsSecondaryButtonEnabled = false;
                        ms.CloseButtonText = "متوجه شدم";
                        await ms.ShowDialogAsync();
                    }
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                    ms.Title = "خطا";
                    ms.Content = "هیچ کاربری تعریف نشده است";
                    ms.IsPrimaryButtonEnabled = false;
                    ms.IsSecondaryButtonEnabled = false;
                    ms.CloseButtonText = "متوجه شدم";
                    await ms.ShowDialogAsync();
                }

            }
            ExitBtn.IsEnabled = true;
            LoginBtn.IsEnabled = true;
            passwordBox.Text = "";
            LoginProgressBar.Visibility = Visibility.Collapsed;
        }
        private bool CheckUsers() => _userManager.Users.Any();
        private async Task<bool> StartSyncJobs()
        {
            if (_synchronizationService != null)
            {
                TServiceResponse<bool> result = new TServiceResponse<bool>();
                List<bool> resultList = new List<bool>();

                SyncBox.Visibility = Visibility.Visible;
                await ChangeSyncJobsState("GetParkingInfo", JobState.Syncing);
                result = await _synchronizationService?.GetParkingLotDetailsFromServerAsync();
                if (result.Succeeded)
                {
                    await ChangeSyncJobsState("GetParkingInfo", JobState.Success); resultList.Add(true);
                }
                else
                    await ChangeSyncJobsState("GetParkingInfo", JobState.Failed);



                await ChangeSyncJobsState("GetUsers", JobState.Syncing);
                result = await _synchronizationService?.GetParkingLotAccountsFromServerAsync();
                if (result.Succeeded)
                {
                    ChangeSyncJobsState("GetUsers", JobState.Success); resultList.Add(true);
                }
                else
                    await ChangeSyncJobsState("GetUsers", JobState.Failed);

                await ChangeSyncJobsState("GetPrices", JobState.Syncing);
                result = await _synchronizationService?.ReceiveVehicleSegmentsListFromServerAsync();
                if (result.Succeeded)
                {
                    await ChangeSyncJobsState("GetPrices", JobState.Success); resultList.Add(true);
                }
                else
                    await ChangeSyncJobsState("GetPrices", JobState.Failed);


                await ChangeSyncJobsState("GetGroups", JobState.Syncing);
                result = await _synchronizationService?.ReceiveLicensePlateGroupFromServerAsync();
                if (result.Succeeded)
                {
                    await ChangeSyncJobsState("GetGroups", JobState.Success); resultList.Add(true);
                }

                else
                    await ChangeSyncJobsState("GetGroups", JobState.Failed);
                if (resultList.Count(a => a == true) == 4)
                    return true;
                return false;
            }
            else
            {
                return false;
            }
        }
        private async Task ChangeSyncJobsState(string jobName, JobState jobState)
        {

            // پیدا کردن DockPanel مربوط به jobName

            await this.Dispatcher.InvokeAsync(() =>
            {
                var dockPanel = SyncBox.Children.OfType<DockPanel>().FirstOrDefault(dp => dp.Name == jobName + "Box");
                if (dockPanel == null)
                {
                    // اگر DockPanel پیدا نشد
                    return;
                }
                // پیدا کردن TextBlock و تغییر رنگ آن براساس وضعیت
                var textBlock = dockPanel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                if (textBlock != null)
                {
                    textBlock.Foreground = GetBrushFromState(jobState);
                }

                // حذف آیکون یا ProgressRing قدیمی (اگر وجود دارد)
                var existingIcon = dockPanel.Children.OfType<SymbolIcon>().FirstOrDefault();
                if (existingIcon != null)
                {
                    dockPanel.Children.Remove(existingIcon);
                }
                var existingProgressRing = dockPanel.Children.OfType<ProgressRing>().FirstOrDefault();
                if (existingProgressRing != null)
                {
                    dockPanel.Children.Remove(existingProgressRing);
                }

                if (jobState == JobState.Syncing)
                {
                    var progressRing = new ProgressRing
                    {
                        IsIndeterminate = true,
                        Width = 13,
                        Height = 13,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                    };
                    DockPanel.SetDock(progressRing, Dock.Right);
                    dockPanel.Children.Add(progressRing);
                }
                else if (jobState == JobState.Success || jobState == JobState.Failed)
                {
                    var icon = new SymbolIcon
                    {
                        Symbol = jobState == JobState.Success ? SymbolRegular.Checkmark12 : SymbolRegular.Warning16,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        Foreground = GetBrushFromState(jobState)
                    };
                    DockPanel.SetDock(icon, Dock.Right);
                    dockPanel.Children.Add(icon);
                }
            }
            );
        }


        private System.Windows.Media.Brush GetBrushFromState(JobState state)
        {
            return state switch
            {
                JobState.Success => (System.Windows.Media.Brush)Application.Current.Resources["SystemFillColorSuccessBrush"],
                JobState.Failed => (System.Windows.Media.Brush)Application.Current.Resources["SystemFillColorCriticalBrush"],
                JobState.Syncing => (System.Windows.Media.Brush)Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                _ => System.Windows.Media.Brushes.Black
            };
        }

        public static void SaveCredentails(string Username, string Password)
        {
            string combined = $"{Username}: {Password}";

            byte[] encrypted = AesEncryption.Encrypt(combined, CrendentialsKey, Iv);
            File.WriteAllBytes(CredenatialsPath, encrypted);
        }

        public static (string Username, string Password)? LoadCredentials()
        {
            if (!File.Exists(CredenatialsPath))
                return null;

            byte[] encrypted = File.ReadAllBytes(CredenatialsPath);

            string decrypted = AesEncryption.Decrypt(encrypted, CrendentialsKey, Iv);
            string[] parts = decrypted.Split(":");
            if (parts.Length == 2)
                return (parts[0].Trim(), parts[1].Trim());

            return null;
        }

        private enum JobState
        {
            None, Syncing, Success, Failed
        }
    }
}
