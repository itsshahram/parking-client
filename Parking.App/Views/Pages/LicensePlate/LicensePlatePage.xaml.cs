namespace Parking.App.Views.Pages.LicensePlate;

public partial class LicensePlatePage : Page
{
    public LicensePlatePageViewModel ViewModel { get; }

    public LicensePlatePage()
    {
        InitializeComponent();
        var parkingService = App.GetService<IParkingService>();
        ViewModel = new LicensePlatePageViewModel(parkingService);
        DataContext = ViewModel;
    }

    private async void AddLicensePlate_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadData();
    }
}

