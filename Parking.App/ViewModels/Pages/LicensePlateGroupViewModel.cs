using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class LicensePlateGroupViewModel : INotifyPropertyChanged
{

    private int _currentPage = 1;
    private int _itemsPerPage = 10;

    public ObservableCollection<LicensePlateListItemViewModel> _items;
    public ObservableCollection<LicensePlateListItemViewModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged(nameof(Items));
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


    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
