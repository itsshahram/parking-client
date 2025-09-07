using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IParkingService _parkingService;
    public MainPageViewModel()
    {

        LatestEntryListItems = new ObservableCollection<TicketsListViewModel>();
        LatestExitedListItems = new ObservableCollection<TicketsListViewModel>();
        _parkingService = App.GetService<IParkingService>();

        //if (!string.IsNullOrEmpty(saved))
        //{
        //    foreach (var desc in saved.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        Descriptions.Add(desc.Trim());
        //}

        //Descriptions.Insert(0, "توضیحات دلخواه");
        LoadDescriptions();
    }
    private void LoadDescriptions()
    {
        var items = _parkingService.GetAllTicketDescriptionItems();
        Descriptions.Clear();
        Descriptions.Add(new TicketDescriptionItemModel { Id = 0, IsQueueEnabled = false, CreateDate = DateTime.Now, Text = "توضیحات دلخواه" });

        SelectedDescription = Descriptions.FirstOrDefault();

        foreach (var item in items)
        {
            Descriptions.Add(item);
        }
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


    private ObservableCollection<TicketDescriptionItemModel> _descriptions = new ObservableCollection<TicketDescriptionItemModel>();
    public ObservableCollection<TicketDescriptionItemModel> Descriptions
    {
        get => _descriptions;
        set { _descriptions = value; OnPropertyChanged(nameof(Descriptions)); }
    }

    private TicketDescriptionItemModel _selectedDescription;
    public TicketDescriptionItemModel SelectedDescription
    {
        get => _selectedDescription;
        set
        {
            if (_selectedDescription != value)
            {
                _selectedDescription = value;
                OnPropertyChanged(nameof(SelectedDescription));

                IsCustomDescriptionVisible = _selectedDescription.Id == 0;

                if (!IsCustomDescriptionVisible)
                {
                    TicketDescription = _selectedDescription.Text;
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