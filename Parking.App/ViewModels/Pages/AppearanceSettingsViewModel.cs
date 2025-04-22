using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Wpf.Ui.Appearance;


namespace Parking.App.ViewModels.Pages;
public partial class AppearanceSettingsViewModel : ObservableObject
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _appVersion = String.Empty;

    [ObservableProperty]
    private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom() { }

    private void InitializeViewModel()
    {
        CurrentTheme = (ApplicationTheme)ApplicationThemeManager.GetAppTheme();
        AppVersion = $"ورژن نرم افزار - {GetAssemblyVersion()}";

        _isInitialized = true;
    }

    private string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? String.Empty;
    }

    [RelayCommand]
    private void OnChangeTheme(string parameter)
    {
;
        switch (parameter)
        {
            case "theme_light":
                if (CurrentTheme == ApplicationTheme.Light)
                    break;

                ApplicationThemeManager.Apply((Wpf.Ui.Appearance.ApplicationTheme)ApplicationTheme.Light);
                CurrentTheme = ApplicationTheme.Light;
                Settings.Default.Appearance_Theme = "theme_light";
                Settings.Default.Save();

                break;
            case "theme_system":
                if (CurrentTheme == ApplicationTheme.Unknown)
                    break;
                Settings.Default.Appearance_Theme = "theme_system";
                Settings.Default.Save();
                break;
            default:
                if (CurrentTheme == ApplicationTheme.Dark)
                    break;

                ApplicationThemeManager.Apply((Wpf.Ui.Appearance.ApplicationTheme)ApplicationTheme.Dark);
                CurrentTheme = ApplicationTheme.Dark;
                Settings.Default.Appearance_Theme = "theme_light";
                Settings.Default.Save();
                break;
        }
    }
}
