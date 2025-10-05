using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;
public class LicensePlatePageViewModel : INotifyPropertyChanged
{
    private readonly IParkingService _parkingService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LicensePlateListItemResult> LicensePlateGroups { get; set; }
        = new ObservableCollection<LicensePlateListItemResult>();

    public ICommand EditCommand { get; }

    public LicensePlatePageViewModel(IParkingService parkingService)
    {
        _parkingService = parkingService;
        _ = LoadData();
    }


    public async Task LoadData()
    {
        var list = await _parkingService.GetLicensePlateList();
        LicensePlateGroups.Clear();
        foreach (var item in list)
        {
            LicensePlateGroups.Add(new LicensePlateListItemResult
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                DiscountPercent = item.DiscountPercent,
                StartDateString = item.StartDate.ToShamsi(),
                EndDateString = item.EndDate.ToShamsi(),
            });
        }
    }
}
