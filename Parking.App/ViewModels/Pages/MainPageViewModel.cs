using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class MainPageViewModel : INotifyPropertyChanged
{

    public MainPageViewModel()
    {

        LatestEntryListItems = new ObservableCollection<TicketsListViewModel>();
        LatestExitedListItems = new ObservableCollection<TicketsListViewModel>();
        var saved = Settings.Default.Application_DefaultTicketDescription;
        if (!string.IsNullOrEmpty(saved))
        {
            foreach (var desc in saved.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                Descriptions.Add(desc.Trim());
        }

        Descriptions.Insert(0, "توضیحات دلخواه");

        SelectedDescription = Descriptions.FirstOrDefault();
    }
    private ComboBoxItem _selectedVehicleSegmentItem;
    public ComboBoxItem SelectedVehicleSegmentItem
    {
        get => _selectedVehicleSegmentItem;
        set
        {
            _selectedVehicleSegmentItem = value;
            OnPropertyChanged(nameof(SelectedVehicleSegmentItem));
        }
    }


    private BitmapSource _currentFrame;
    public BitmapSource CurrentFrame
    {
        get => _currentFrame;
        set
        {
            _currentFrame = value;
            OnPropertyChanged(nameof(CurrentFrame));
        }
    }



    private string _driverPhoneNumber;
    public string DriverPhoneNumber
    {
        get => _driverPhoneNumber;
        set
        {
            if (_driverPhoneNumber != value)
            {
                _driverPhoneNumber = value;
                OnPropertyChanged(nameof(DriverPhoneNumber));
            }
        }
    }
    private string _driverFullName;
    public string DriverFullName
    {
        get => _driverFullName;
        set
        {
            if (_driverFullName != value)
            {
                _driverFullName = value;
                OnPropertyChanged(nameof(DriverFullName));
            }
        }
    }

    private string _driverDescription;
    public string DriverDescription
    {
        get => _driverDescription;
        set
        {
            if (_driverDescription != value)
            {
                _driverDescription = value;
                OnPropertyChanged(nameof(DriverDescription));
            }
        }
    }

    private string _ticketDescription;
    public string TicketDescription
    {
        get => _ticketDescription;
        set
        {
            if (_ticketDescription != value)
            {
                _ticketDescription = value;
                OnPropertyChanged(nameof(TicketDescription));
            }
        }
    }


    private ObservableCollection<string> _descriptions = new ObservableCollection<string>();
    public ObservableCollection<string> Descriptions
    {
        get => _descriptions;
        set { _descriptions = value; OnPropertyChanged(nameof(Descriptions)); }
    }

    private string _selectedDescription;
    public string SelectedDescription
    {
        get => _selectedDescription;
        set
        {
            if (_selectedDescription != value)
            {
                _selectedDescription = value;
                OnPropertyChanged(nameof(SelectedDescription));

                IsCustomDescriptionVisible = _selectedDescription == "توضیحات دلخواه";

                if (!IsCustomDescriptionVisible)
                {
                    TicketDescription = _selectedDescription;
                }
            }
        }
    }

    private bool _isCustomDescriptionVisible;
    public bool IsCustomDescriptionVisible
    {
        get => _isCustomDescriptionVisible;
        set { _isCustomDescriptionVisible = value; OnPropertyChanged(nameof(IsCustomDescriptionVisible)); }
    }


    private ObservableCollection<TicketsListViewModel> latestEntryListItems;
    public ObservableCollection<TicketsListViewModel> LatestEntryListItems
    {
        get => latestEntryListItems;
        set
        {
            latestEntryListItems = value;
            OnPropertyChanged(nameof(LatestEntryListItems));
        }
    }

    private ObservableCollection<TicketsListViewModel> latestExitedListItems;
    public ObservableCollection<TicketsListViewModel> LatestExitedListItems
    {
        get => latestExitedListItems;
        set
        {
            latestExitedListItems = value;
            OnPropertyChanged(nameof(LatestExitedListItems));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CameraImageModel> CameraImages { get; set; } = new ObservableCollection<CameraImageModel>();


    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}