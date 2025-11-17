using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Parking.App.ViewModels.Pages;

public class LicensePlateGroupCardViewModel : INotifyPropertyChanged
{
    private ObservableCollection<LicensePlatePlateItemViewModel> _plates = new();

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ObservableCollection<LicensePlatePlateItemViewModel> Plates
    {
        get => _plates;
        set
        {
            if (_plates != value)
            {
                _plates = value;
                OnPropertyChanged();
            }
        }
    }

    public string StartDateString => StartDate.ToShamsi(includeTime: true);
    public string EndDateString => EndDate.ToShamsi(includeTime: true);
    public string StatusText => IsActive ? "فعال" : "غیرفعال";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class LicensePlatePlateItemViewModel
{
    public Guid Id { get; set; }
    public string PersianPlate { get; set; } = string.Empty;
    public string EnglishPlate { get; set; } = string.Empty;
}

public class LicensePlatePageViewModel : INotifyPropertyChanged
{
    private readonly IParkingService _parkingService;
    private int _currentPage = 1;
    private int _itemsPerPage = 10;
    private int _totalCount;
    private int _totalGroups;
    private int _totalActiveGroups;
    private int _totalInactiveGroups;
    private string? _filterName;
    private int? _filterDiscount;
    private DateTime? _filterStartDate;
    private DateTime? _filterEndDate;
    private string _startYear = string.Empty;
    private string _startMonth = string.Empty;
    private string _startDay = string.Empty;
    private string _startHour = string.Empty;
    private string _startMinute = string.Empty;
    private string _endYear = string.Empty;
    private string _endMonth = string.Empty;
    private string _endDay = string.Empty;
    private string _endHour = string.Empty;
    private string _endMinute = string.Empty;

    public LicensePlatePageViewModel(IParkingService parkingService)
    {
        _parkingService = parkingService;
        LicensePlateGroups = new ObservableCollection<LicensePlateGroupCardViewModel>();
    }

    public ObservableCollection<LicensePlateGroupCardViewModel> LicensePlateGroups { get; }

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }
    }

    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            if (_itemsPerPage != value)
            {
                _itemsPerPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount != value)
            {
                _totalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
            }
        }
    }

    public int TotalPages => ItemsPerPage <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / ItemsPerPage);

    public int TotalGroups
    {
        get => _totalGroups;
        set
        {
            if (_totalGroups != value)
            {
                _totalGroups = value;
                OnPropertyChanged();
            }
        }
    }

    public int TotalActiveGroups
    {
        get => _totalActiveGroups;
        set
        {
            if (_totalActiveGroups != value)
            {
                _totalActiveGroups = value;
                OnPropertyChanged();
            }
        }
    }

    public int TotalInactiveGroups
    {
        get => _totalInactiveGroups;
        set
        {
            if (_totalInactiveGroups != value)
            {
                _totalInactiveGroups = value;
                OnPropertyChanged();
            }
        }
    }

    public string? FilterName
    {
        get => _filterName;
        set
        {
            if (_filterName != value)
            {
                _filterName = value;
                OnPropertyChanged();
            }
        }
    }

    public int? FilterDiscount
    {
        get => _filterDiscount;
        set
        {
            if (_filterDiscount != value)
            {
                _filterDiscount = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? FilterStartDate
    {
        get => _filterStartDate;
        private set
        {
            if (_filterStartDate != value)
            {
                _filterStartDate = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? FilterEndDate
    {
        get => _filterEndDate;
        private set
        {
            if (_filterEndDate != value)
            {
                _filterEndDate = value;
                OnPropertyChanged();
            }
        }
    }

    #region Date filter components

    public string StartYear { get => _startYear; set { if (_startYear != value) { _startYear = value; OnPropertyChanged(); UpdateStartDate(); } } }
    public string StartMonth { get => _startMonth; set { if (_startMonth != value) { _startMonth = value; OnPropertyChanged(); UpdateStartDate(); } } }
    public string StartDay { get => _startDay; set { if (_startDay != value) { _startDay = value; OnPropertyChanged(); UpdateStartDate(); } } }
    public string StartHour { get => _startHour; set { if (_startHour != value) { _startHour = value; OnPropertyChanged(); UpdateStartDate(); } } }
    public string StartMinute { get => _startMinute; set { if (_startMinute != value) { _startMinute = value; OnPropertyChanged(); UpdateStartDate(); } } }

    public string EndYear { get => _endYear; set { if (_endYear != value) { _endYear = value; OnPropertyChanged(); UpdateEndDate(); } } }
    public string EndMonth { get => _endMonth; set { if (_endMonth != value) { _endMonth = value; OnPropertyChanged(); UpdateEndDate(); } } }
    public string EndDay { get => _endDay; set { if (_endDay != value) { _endDay = value; OnPropertyChanged(); UpdateEndDate(); } } }
    public string EndHour { get => _endHour; set { if (_endHour != value) { _endHour = value; OnPropertyChanged(); UpdateEndDate(); } } }
    public string EndMinute { get => _endMinute; set { if (_endMinute != value) { _endMinute = value; OnPropertyChanged(); UpdateEndDate(); } } }

    private void UpdateStartDate()
    {
        FilterStartDate = BuildDate(_startYear, _startMonth, _startDay, _startHour, _startMinute);
    }

    private void UpdateEndDate()
    {
        FilterEndDate = BuildDate(_endYear, _endMonth, _endDay, _endHour, _endMinute);
    }

    private static DateTime? BuildDate(string year, string month, string day, string hour, string minute)
    {
        if (int.TryParse(year, out var y) && int.TryParse(month, out var m) && int.TryParse(day, out var d))
        {
            var h = int.TryParse(hour, out var hh) ? hh : 0;
            var min = int.TryParse(minute, out var mm) ? mm : 0;
            try
            {
                return new DateTime(y, m, d, h, min, 0);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public void ClearFilters()
    {
        FilterName = string.Empty;
        FilterDiscount = null;
        StartYear = StartMonth = StartDay = StartHour = StartMinute = string.Empty;
        EndYear = EndMonth = EndDay = EndHour = EndMinute = string.Empty;
        FilterStartDate = null;
        FilterEndDate = null;
    }

    #endregion

    public async Task LoadDataAsync()
    {
        var result = await _parkingService.GetLicensePlateGroupWithPlatesPaginatedList(
            CurrentPage,
            ItemsPerPage,
            FilterName,
            FilterDiscount,
            FilterStartDate,
            FilterEndDate);

        TotalCount = result.TotalCount;
        TotalGroups = result.TotalGroups;
        TotalActiveGroups = result.TotalActiveGroups;
        TotalInactiveGroups = result.TotalInactiveGroups;

        LicensePlateGroups.Clear();
        foreach (var dto in result.Data)
        {
            var groupVm = new LicensePlateGroupCardViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                IsActive = dto.IsActive,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Plates = new ObservableCollection<LicensePlatePlateItemViewModel>(
                    dto.Plates.Select(p => new LicensePlatePlateItemViewModel
                    {
                        Id = p.Id,
                        PersianPlate = p.PersianPlate,
                        EnglishPlate = p.EnglishPlate
                    }))
            };
            LicensePlateGroups.Add(groupVm);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
