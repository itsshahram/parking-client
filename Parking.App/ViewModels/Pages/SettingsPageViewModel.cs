namespace Parking.App.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<object> _menuItems = new()
    {
        new NavigationViewItem()
        {
            Content = "تنظیمات نمایشی",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Desktop24 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.AppearanceSettingsPage)
        },
        new NavigationViewItem()
        {
            Content = "تنظیمات دوربین",
            Icon = new SymbolIcon { Symbol = SymbolRegular.CameraDome24 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.CameraSettingsPage)
        },
        new NavigationViewItem()
        {
            Content = "تنظیمات کارتخوان",
            Icon = new SymbolIcon { Symbol = SymbolRegular.CreditCardToolbox24 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.POSSettingsPage)
        },
        new NavigationViewItem()
        {
            Content = "تنظیمات اپلیکیشن",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Apps24 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.ApplicationSettingsPage)
        } ,
                new NavigationViewItem()
        {
            Content = "تنظیمات کیبورد",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Key24 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.HotKeyManagment)
        } ,
        new NavigationViewItem()
        {
            Content = "تنظیمات کاربری",
            Icon = new SymbolIcon { Symbol = SymbolRegular.PersonSettings20 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.AccountsSettingsPage)
        },
        new NavigationViewItem()
        {
            Content = "تنظیمات همگام سازی",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Cloud16 },
            TargetPageType = typeof(Views.Pages.SettingsPageChilds.SyncConfigPage)
        }
    };
    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems = new()
    {
        //new NavigationViewItem()
        //{
        //    Content = "خروج از حساب کاربری",
        //    Foreground = new SolidColorBrush(Colors.Red),
        //    Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowExit20 },
        //    TargetPageType = typeof(Views.Pages.SettingsPage)
        //}
    };
}
