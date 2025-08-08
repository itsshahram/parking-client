using CommunityToolkit.Mvvm.ComponentModel;
using Parking.App.Views.Pages.CardsPageChilds;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

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
            TargetPageType = typeof(Views.Pages.MainPage)
        },
        new NavigationViewItem()
        {
            Content = "توقیفی ها",
            Icon = new SymbolIcon { Symbol = SymbolRegular.VehicleCarParking24 },
            TargetPageType = typeof(Views.Pages.SeizedPlatePage)
        },
        new NavigationViewItem()
        {
            Content = "گزارش",
            Icon = new SymbolIcon { Symbol = SymbolRegular.History24 },
            TargetPageType = typeof(Views.Pages.TicketHistoryPage)
        },
        new NavigationViewItem()
        {
            Content = "تعرفه",
            Icon = new SymbolIcon { Symbol = SymbolRegular.ClipboardBulletListLtr20 },
            TargetPageType = typeof(Views.Pages.TariffListPage)
        },
        new NavigationViewItem()
        {
            Content = "گروه ها",
            Icon = new SymbolIcon { Symbol = SymbolRegular.AlignSpaceEvenlyVertical20 },
            TargetPageType = typeof(Views.Pages.LicensePlateGroupPage)
        },
        new NavigationViewItem()
        {
            Content = "اطلاعات",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Info24 },
            TargetPageType = typeof(Views.Pages.InformationPage)
        }
        
    };

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
                TargetPageType = typeof(Views.Pages.UsersListPage)
            });
        }

        if(PermissionHelper.CheckUserPermission(TokenStore.RoleName, "AddCards") && Settings.Default.Application_EntryCardRequirement)
        {
            _menuItems.Add(new NavigationViewItem()
            {
                Content = "کارت",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ContactCardGroup24 },
                TargetPageType = typeof(Views.Pages.AddCardPage)
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
                TargetPageType = typeof(Views.Pages.SettingsPage)
            });
        }
    }





    //[ObservableProperty]
    //private ObservableCollection<MenuItem> _trayMenuItems = new()
    //{
    //    new MenuItem { Header = "Home", Tag = "tray_home" }
    //};

}
