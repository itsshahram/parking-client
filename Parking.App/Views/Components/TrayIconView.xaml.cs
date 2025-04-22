

namespace Parking.App.Views.Components;

[ObservableObject]
/// <summary>
/// Interaction logic for TrayIconView.xaml
/// </summary>
public partial class TrayIconView : UserControl
{
    private IThemeService _themeService;
    private string _imagePath;
    [ObservableProperty]
    private bool _isWindowVisible = false;

    public TrayIconViewModel ViewModel { get; }
    public TrayIconView()
    {
        ViewModel = new TrayIconViewModel();
        DataContext = ViewModel;
        InitializeComponent();
        //string imagePath = "Assets/ServerIconUpdate.ico";
        _themeService = App.GetService<IThemeService>();

        _imagePath = "Assets/icon.ico";
        var nativeTheme = _themeService.GetNativeSystemTheme();
        if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Dark)
        {
            _imagePath = "Assets/car-light.ico";
        } else if(nativeTheme == Wpf.Ui.Appearance.SystemTheme.Light)
        {
            _imagePath = "Assets/car-dark.ico";
        }
        else
        {
            _imagePath = "Assets/icon.ico";
        }


        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(_imagePath, UriKind.Relative);
        bitmap.EndInit();
        ViewModel.IconSource = bitmap;

    }

    [RelayCommand]
    public void ShowHideWindow()
    {

    }

    [RelayCommand]
    public void ExitApplication()
    {

    }
    public void ChangeIcon(string imagePath)
    {
        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(imagePath, UriKind.Relative);
        bitmap.EndInit();
        ViewModel.IconSource = bitmap;
    }
    public void ResetIcon()
    {
        var nativeTheme = _themeService.GetNativeSystemTheme();
        if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Dark)
        {
            _imagePath = "Assets/car-light.ico";
        }
        else if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Light)
        {
            _imagePath = "Assets/car-dark.ico";
        }
        else
        {
            _imagePath = "Assets/icon.ico";
        }

        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(_imagePath, UriKind.Relative);
        bitmap.EndInit();
        ViewModel.IconSource = bitmap;
        notifyTray.TooltipText = "Madaar Parking Manager Application";
    }
    public void SetSyncingIcon()
    {
        var nativeTheme = _themeService.GetNativeSystemTheme();
        if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Dark)
        {
            _imagePath = "Assets/cloud-light.ico";
        }
        else if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Light)
        {
            _imagePath = "Assets/cloud-dark.ico";
        }
        else
        {
            _imagePath = "Assets/ServerIconUpdate.ico";
        }

        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(_imagePath, UriKind.Relative);
        bitmap.EndInit();
        ViewModel.IconSource = bitmap;
        notifyTray.TooltipText = "Synchronizing with the server...";

    }
    public void SetErrorIcon(string errorMessage)
    {
        var nativeTheme = _themeService.GetNativeSystemTheme();
        if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Dark)
        {
            _imagePath = "Assets/car-red.ico";
        }
        else if (nativeTheme == Wpf.Ui.Appearance.SystemTheme.Light)
        {
            _imagePath = "Assets/car-red.ico";
        }
        else
        {
            _imagePath = "Assets/car-red.ico";
        }

        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(_imagePath, UriKind.Relative);
        bitmap.EndInit();
        ViewModel.IconSource = bitmap;
        notifyTray.TooltipText = errorMessage;
    }
    //private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
    //{
    //    string imagePath = "Assets/ServerIconUpdate.ico";

    //    // بارگذاری تصویر به عنوان ImageSource
    //    BitmapImage bitmap = new BitmapImage();
    //    bitmap.BeginInit();
    //    bitmap.UriSource = new Uri(imagePath, UriKind.Relative);
    //    bitmap.EndInit();

    //    notifyTray.Icon = bitmap;
    //}


    private void ShowWindowBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var app = (App)App.Current;
        app.ShowMainWindow();
    }

    private void CloseAppBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        App.GlobalCancellationTokenSource.Cancel();
        App.GlobalCancellationTokenSource.Dispose();

        var app = (App)App.Current;
        app.Shutdown();
    }
}
