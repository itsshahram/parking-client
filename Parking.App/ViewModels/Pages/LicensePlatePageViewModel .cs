using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Parking.App.ViewModels.Pages
{
    public class LicensePlatePageViewModel : INotifyPropertyChanged
    {
        private readonly IParkingService _parkingService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalCount;
        private int _totalGroups;
        private int _totalActiveGroups;
        private int _totalInactiveGroups;

        public ObservableCollection<LicensePlateListItemResult> LicensePlateGroups { get; }
            = new ObservableCollection<LicensePlateListItemResult>();

        public ICommand EditCommand { get; }

        public LicensePlatePageViewModel(IParkingService parkingService)
        {
            _parkingService = parkingService;
            _ = LoadDataAsync();
        }

        #region Summary Properties

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

        #endregion

        #region Pagination Properties

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

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / ItemsPerPage);

        #endregion

        #region Data Loading

        // Start Date Components
        private string _startYear = "";
        public string StartYear { get => _startYear; set { _startYear = value; UpdateStartDate(); OnPropertyChanged(); } }

        private string _startMonth = "";
        public string StartMonth { get => _startMonth; set { _startMonth = value; UpdateStartDate(); OnPropertyChanged(); } }

        private string _startDay = "";
        public string StartDay { get => _startDay; set { _startDay = value; UpdateStartDate(); OnPropertyChanged(); } }

        private string _startHour = "";
        public string StartHour { get => _startHour; set { _startHour = value; UpdateStartDate(); OnPropertyChanged(); } }

        private string _startMinute = "";
        public string StartMinute { get => _startMinute; set { _startMinute = value; UpdateStartDate(); OnPropertyChanged(); } }

        private string _endYear = "";
        public string EndYear { get => _endYear; set { _endYear = value; UpdateEndDate(); OnPropertyChanged(); } }

        private string _endMonth = "";
        public string EndMonth { get => _endMonth; set { _endMonth = value; UpdateEndDate(); OnPropertyChanged(); } }

        private string _endDay = "";
        public string EndDay { get => _endDay; set { _endDay = value; UpdateEndDate(); OnPropertyChanged(); } }

        private string _endHour = "";
        public string EndHour { get => _endHour; set { _endHour = value; UpdateEndDate(); OnPropertyChanged(); } }

        private string _endMinute = "";
        public string EndMinute { get => _endMinute; set { _endMinute = value; UpdateEndDate(); OnPropertyChanged(); } }

        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }

        private void UpdateStartDate()
        {
            if (int.TryParse(StartYear, out var y) &&
                int.TryParse(StartMonth, out var m) &&
                int.TryParse(StartDay, out var d))
            {
                int hour = int.TryParse(StartHour, out var h) ? h : 0;
                int minute = int.TryParse(StartMinute, out var min) ? min : 0;

                try
                {
                    FilterStartDate = new DateTime(y, m, d, hour, minute, 0);
                }
                catch
                {
                    FilterStartDate = null;
                }
            }
            else
            {
                FilterStartDate = null;
            }
            OnPropertyChanged(nameof(FilterStartDate));
        }

        private void UpdateEndDate()
        {
            if (int.TryParse(EndYear, out var y) &&
                int.TryParse(EndMonth, out var m) &&
                int.TryParse(EndDay, out var d))
            {
                int hour = int.TryParse(EndHour, out var h) ? h : 0;
                int minute = int.TryParse(EndMinute, out var min) ? min : 0;

                try
                {
                    FilterEndDate = new DateTime(y, m, d, hour, minute, 0);
                }
                catch
                {
                    FilterEndDate = null;
                }
            }
            else
            {
                FilterEndDate = null;
            }
            OnPropertyChanged(nameof(FilterEndDate));
        }

        #endregion

        #region Data Loading

        public async Task LoadDataAsync()
        {
            var listResult = await _parkingService.GetLicensePlatePaginatedList(
                CurrentPage,
                ItemsPerPage,
                FilterName,
                FilterDiscount,
                FilterStartDate,
                FilterEndDate);

            TotalGroups = await _parkingService.GetTotalLicensePlateGroupCountAsync();
            TotalActiveGroups = await _parkingService.GetActiveLicensePlateGroupCountAsync();
            TotalInactiveGroups = await _parkingService.GetInactiveLicensePlateGroupCountAsync();

            TotalCount = listResult.TotalCount;

            LicensePlateGroups.Clear();
            foreach (var item in listResult.Data)
            {
                LicensePlateGroups.Add(new LicensePlateListItemResult
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    DiscountPercent = item.DiscountPercent,
                });
            }
        }

        #endregion

        #region Optional Filters
        public string? FilterName { get; set; }
        public int? FilterDiscount { get; set; }

        #endregion
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
