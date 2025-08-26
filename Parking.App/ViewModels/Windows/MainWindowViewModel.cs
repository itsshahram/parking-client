using Parking.App.Views.Pages.CardsPageChilds;

namespace Parking.App.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "نرم افزار مدیریت پارکینگ مدار";

    [ObservableProperty]
    private ObservableCollection<object> _menuItems = new()
    {
        new NavigationViewItem()
        {
            Content = "ورود",
            Icon = new SymbolIcon { Symbol = SymbolRegular.VehicleCar24 },
            TargetPageType = typeof(MainPage)
        },
        new NavigationViewItem()
        {
            Content = "توقیفی ها",
            Icon = new SymbolIcon { Symbol = SymbolRegular.VehicleCarParking24 },
            TargetPageType = typeof(SeizedPlatePage)
        },
        new NavigationViewItem()
        {
            Content = "گزارش",
            Icon = new SymbolIcon { Symbol = SymbolRegular.History24 },
            TargetPageType = typeof(TicketHistoryPage)
        },
        new NavigationViewItem()
        {
            Content = "تعرفه",
            Icon = new SymbolIcon { Symbol = SymbolRegular.ClipboardBulletListLtr20 },
            TargetPageType = typeof(TariffListPage)
        },
        new NavigationViewItem()
        {
            Content = "گروه ها",
            Icon = new SymbolIcon { Symbol = SymbolRegular.AlignSpaceEvenlyVertical20 },
            TargetPageType = typeof(LicensePlateGroupPage)
        },
        new NavigationViewItem()
        {
            Content = "اطلاعات",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Info24 },
            TargetPageType = typeof(InformationPage)
        }
    };

    private static readonly string AppDataFolder =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parking.App");


    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems = new();
    public MainWindowViewModel()
    {
        Initialize();
    }

    public void Initialize()
    {
        var s = TokenStore.RoleName;
        if (PermissionHelper.CheckUserPermission(TokenStore.RoleName, "UserManagement"))
        {
            _menuItems.Add(new NavigationViewItem()
            {
                Content = "کاربران",
                Icon = new SymbolIcon { Symbol = SymbolRegular.PersonSettings20 },
                TargetPageType = typeof(UsersListPage)
            });
        }

        if (PermissionHelper.CheckUserPermission(TokenStore.RoleName, "AddCards") && Settings.Default.Application_EntryCardRequirement)
        {
            _menuItems.Add(new NavigationViewItem()
            {
                Content = "کارت",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ContactCardGroup24 },
                TargetPageType = typeof(AddCardPage)
            });
        }
        if (PermissionHelper.CheckUserPermission(TokenStore.RoleName, "AddCards") && Settings.Default.Application_EntryCardRequirement)
        {
            _menuItems.Add(new NavigationViewItem()
            {
                Content = "تاریخچه کارت",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ContactCardGroup24 },
                TargetPageType = typeof(AddCardHistoryPage)
            });
        }
        if (PermissionHelper.CheckUserPermission(TokenStore.RoleName, "ApplicationSettings"))
        {
            FooterMenuItems.Add(new NavigationViewItem()
            {
                Content = "تنظیمات",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(SettingsPage)
            });
        }
            FooterMenuItems.Add(new NavigationViewItem()
            {
                Content = "خروج",
                Icon = new SymbolIcon { Symbol = SymbolRegular.SignOut24 },
                Tag = "Logout",
                Command = new CommunityToolkit.Mvvm.Input.RelayCommand(HandleLogout)
            });
    }
    private void HandleLogout()
    {
        TokenStore.Clear();

        string credentialsPath = Path.Combine(AppDataFolder, "credentials.dat");
        if (File.Exists(credentialsPath))
            File.Delete(credentialsPath);

        string keyPath = Path.Combine(AppDataFolder, "aeskey.bin");
        if (File.Exists(keyPath))
            File.Delete(keyPath);

        var windowsToClose = Application.Current.Windows.Cast<Window>()
            .Where(w => !(w is LoginWindow)).ToList();

        foreach (var w in windowsToClose)
            w.Close();

        Application.Current.Dispatcher.BeginInvoke(async () =>
        {

            var loginWindow = App.GetService<LoginWindow>() ?? new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }
}

