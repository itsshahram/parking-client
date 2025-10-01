using Parking.App.Views.Windows.LicensePlate;
using Parking.Domain.Entities.Vehicles;

namespace Parking.App.Views.Pages.LicensePlate;

public partial class LicensePlatePage : Page
{
    private readonly IParkingService _parkingService;
    public LicensePlatePageViewModel ViewModel { get; }

    public LicensePlatePage()
    {
        InitializeComponent();
        _parkingService = App.GetService<IParkingService>();
        ViewModel = new LicensePlatePageViewModel(_parkingService);
        DataContext = ViewModel;
    }

    private async void AddLicensePlate_Click(object sender, RoutedEventArgs e)
    {
        AddLicensePlateWindows windows = new AddLicensePlateWindows();
        windows.Owner = Application.Current.MainWindow;

        if (windows.ShowDialog() == true)
        {
            var licensePlateGroup = windows.Vm;
            await _parkingService.AddLicensePlateGroup(new LicensePlateGroup()
            {
                Name = licensePlateGroup.Name,
                StartDate = licensePlateGroup.StartDate,
                Description = licensePlateGroup.Description,
                DiscountPercent = licensePlateGroup.DiscountPercent,
                EndDate = licensePlateGroup.EndDate.Value,
            });
            await ViewModel.LoadData();
        }
    }
}

