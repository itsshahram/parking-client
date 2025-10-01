namespace Parking.App.Views.Pages.LicensePlate;

/// <summary>
/// Interaction logic for LicensePlateTabsPage.xaml
/// </summary>
public partial class LicensePlateTabsPage : Page
{
    public LicensePlateTabsPage()
    {
        InitializeComponent();

        LicensePlateFrame.Navigate(new LicensePlatePage());
        LicensePlateGroupFrame.Navigate(new LicensePlateGroupPage());
    }
}
