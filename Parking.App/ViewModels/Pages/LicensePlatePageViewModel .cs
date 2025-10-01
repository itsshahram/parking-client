using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;


public class LicensePlatePageViewModel : INotifyPropertyChanged
{
    private readonly IParkingService _parkingService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LicensePlateListItemResult> LicensePlateGroups { get; set; }
        = new ObservableCollection<LicensePlateListItemResult>();

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public LicensePlatePageViewModel(IParkingService parkingService)
    {
        _parkingService = parkingService;

        EditCommand = new RelayCommand<LicensePlateListItemResult>(Edit);
        DeleteCommand = new RelayCommand<LicensePlateListItemResult>(Delete);

        _ = LoadData();
    }

    private async void Edit(LicensePlateListItemResult? item)
    {
        if (item == null) return;
        await Task.CompletedTask;
    }

    private async void Delete(LicensePlateListItemResult? item)
    {
        if (item == null) return;
        //await _parkingService.DeleteLicensePlateGroup(item.Name);
        LicensePlateGroups.Remove(item);
    }

    public async Task LoadData()
    {
        var list = await _parkingService.GetLicensePlateList();
        LicensePlateGroups.Clear();
        foreach (var item in list)
        {
            LicensePlateGroups.Add(new LicensePlateListItemResult
            {
                Name = item.Name,
                Description = item.Description,
                DiscountPercent = item.DiscountPercent,
                StartDateString = item.StartDate.ToShamsi(),
                EndDateString = item.EndDate.ToShamsi()
            });
        }
    }
}
