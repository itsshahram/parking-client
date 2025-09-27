


using Parking.App.Models.Dto.Card;
using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class AddCardHistoryPageViewModel : INotifyPropertyChanged
{
    private int _currentPage = 1;
    private int _itemsPerPage = 10;
    private ObservableCollection<AddCardItemModel> _items;


    public event PropertyChangedEventHandler? PropertyChanged;


    private string? _fullname;
    private string? _licensePlate;
    private string? _cardUidTextBox;

    private DateTime? _startTime;
    private DateTime? _endTime;

    public string? LicensePlate
    {
        get => _licensePlate;
        set
        {
            if (_licensePlate != value)
            {
                _licensePlate = value;
                OnPropertyChanged(nameof(LicensePlate));

            }
        }
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime != value)
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime != value)
            {
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }
    }

    public string? CardUidTextBox
    {
        get => _cardUidTextBox;
        set
        {
            if (_cardUidTextBox != value)
            {
                _cardUidTextBox = value;
                OnPropertyChanged(nameof(CardUidTextBox));

            }
        }
    }
    public string? FullName
    {
        get => _fullname;
        set
        {
            if (_fullname != value)
            {
                _fullname = value;
                OnPropertyChanged(nameof(FullName));

            }
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));

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
                OnPropertyChanged(nameof(ItemsPerPage));
            }
        }
    }
    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount != value)
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(TotalPages));
            }
        }
    }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / _itemsPerPage);

    public ObservableCollection<AddCardItemModel> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
                OnPropertyChanged(nameof(LicensePlate));
            }
        }
    }

    public AddCardHistoryPageViewModel()
    {
        _parkingService = App.GetService<IParkingService>();
    }
    private readonly IParkingService? _parkingService;



    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ICommand NextPageCommand => new Utilities.RelayCommand(() =>
    {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    });

    public ICommand PreviousPageCommand => new Utilities.RelayCommand(() =>
    {
        if (CurrentPage > 1)
            CurrentPage--;
    });
}