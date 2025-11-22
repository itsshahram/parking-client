using Parking.Domain.General;
using System.ComponentModel;

public class GetTicketListRequestModel : INotifyPropertyChanged
{
    private string? _licensePlate;
    private int? _parkingId;
    private Guid? _sectionId;
    private long? _barcodeId;
    private int? _vehicleSegmentId;
    private Guid? _vehicleId;
    private Guid? _vehicleOwnerId;
    private Guid? _parkingSpaceID;
    private string? _paidType;
    private int? _minDurationMinutes;
    private int? _maxDurationMinutes;
    private decimal? _minTotalAmount;
    private decimal? _maxTotalAmount;
    private string? _gateType;
    private bool? _isExited;
    private bool? _isPaid;
    private DateTime? _entryFrom;
    private DateTime? _entryTo;
    private DateTime? _exitFrom;
    private DateTime? _exitTo;
    private decimal? _priceFrom;
    private decimal? _priceTo;
    private string? _entryRegistrar;
    private string? _exitRegistrar;
    private bool? _hasDiscrepancy;
    private string? _rrn;
    private string? _trackNo;
    private int? _discount;

    private TicketStatus? _ticketStatus;
    private VehicleStatus? _vehicleStatus;

    private int _currentPage = 1;
    private int _itemsPerPage = 10;
    private int _totalCount;

    private ObservableCollection<TicketsListViewModel> _items;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public int? ParkingId
    {
        get => _parkingId;
        set
        {
            if (_parkingId != value)
            {
                _parkingId = value;
                OnPropertyChanged(nameof(ParkingId));
            }
        }
    }

    public string? TrackNo
    {
        get => _trackNo;
        set
        {
            if (_trackNo != value)
            {
                _trackNo = value;
                OnPropertyChanged(nameof(TrackNo));
            }
        }
    }

    public string? RRN
    {
        get => _rrn;
        set
        {
            if (_rrn != value)
            {
                _rrn = value;
                OnPropertyChanged(nameof(RRN));
            }
        }
    }


    public Guid? SectionId
    {
        get => _sectionId;
        set
        {
            if (_sectionId != value)
            {
                _sectionId = value;
                OnPropertyChanged(nameof(SectionId));
            }
        }
    }

    public long? BarcodeId
    {
        get => _barcodeId;
        set
        {
            if (_barcodeId != value)
            {
                _barcodeId = value;
                OnPropertyChanged(nameof(BarcodeId));
            }
        }
    }

    public int? VehicleSegmentId
    {
        get => _vehicleSegmentId;
        set
        {
            if (_vehicleSegmentId != value)
            {
                _vehicleSegmentId = value;
                OnPropertyChanged(nameof(VehicleSegmentId));
            }
        }
    }

    public Guid? VehicleId
    {
        get => _vehicleId;
        set
        {
            if (_vehicleId != value)
            {
                _vehicleId = value;
                OnPropertyChanged(nameof(VehicleId));
            }
        }
    }

    public Guid? VehicleOwnerId
    {
        get => _vehicleOwnerId;
        set
        {
            if (_vehicleOwnerId != value)
            {
                _vehicleOwnerId = value;
                OnPropertyChanged(nameof(VehicleOwnerId));
            }
        }
    }

    public Guid? ParkingSpaceID
    {
        get => _parkingSpaceID;
        set
        {
            if (_parkingSpaceID != value)
            {
                _parkingSpaceID = value;
                OnPropertyChanged(nameof(ParkingSpaceID));
            }
        }
    }

    public string? PaidType
    {
        get => _paidType;
        set
        {
            if (_paidType != value)
            {
                _paidType = value;
                OnPropertyChanged(nameof(PaidType));
            }
        }
    }

    public int? MinDurationMinutes
    {
        get => _minDurationMinutes;
        set
        {
            if (_minDurationMinutes != value)
            {
                _minDurationMinutes = value;
                OnPropertyChanged(nameof(MinDurationMinutes));
            }
        }
    }

    public int? MaxDurationMinutes
    {
        get => _maxDurationMinutes;
        set
        {
            if (_maxDurationMinutes != value)
            {
                _maxDurationMinutes = value;
                OnPropertyChanged(nameof(MaxDurationMinutes));
            }
        }
    }

    public decimal? MinTotalAmount
    {
        get => _minTotalAmount;
        set
        {
            if (_minTotalAmount != value)
            {
                _minTotalAmount = value;
                OnPropertyChanged(nameof(MinTotalAmount));
            }
        }
    }

    public decimal? MaxTotalAmount
    {
        get => _maxTotalAmount;
        set
        {
            if (_maxTotalAmount != value)
            {
                _maxTotalAmount = value;
                OnPropertyChanged(nameof(MaxTotalAmount));
            }
        }
    }

    public string? GateType
    {
        get => _gateType;
        set
        {
            if (_gateType != value)
            {
                _gateType = value;
                OnPropertyChanged(nameof(GateType));
            }
        }
    }

    public bool? IsExited
    {
        get => _isExited;
        set
        {
            if (_isExited != value)
            {
                _isExited = value;
                OnPropertyChanged(nameof(IsExited));
            }
        }
    }

    public bool? IsPaid
    {
        get => _isPaid;
        set
        {
            if (_isPaid != value)
            {
                _isPaid = value;
                OnPropertyChanged(nameof(IsPaid));
            }
        }
    }

    public DateTime? EntryFrom
    {
        get => _entryFrom;
        set
        {
            if (_entryFrom != value)
            {
                _entryFrom = value;
                OnPropertyChanged(nameof(EntryFrom));
            }
        }
    }

    public DateTime? EntryTo
    {
        get => _entryTo;
        set
        {
            if (_entryTo != value)
            {
                _entryTo = value;
                OnPropertyChanged(nameof(EntryTo));
            }
        }
    }

    public DateTime? ExitFrom
    {
        get => _exitFrom;
        set
        {
            if (_exitFrom != value)
            {
                _exitFrom = value;
                OnPropertyChanged(nameof(ExitFrom));
            }
        }
    }

    public DateTime? ExitTo
    {
        get => _exitTo;
        set
        {
            if (_exitTo != value)
            {
                _exitTo = value;
                OnPropertyChanged(nameof(ExitTo));
            }
        }
    }

    public TicketStatus? TicketStatus
    {
        get => _ticketStatus;
        set
        {
            if (_ticketStatus != value)
            {
                _ticketStatus = value;
                OnPropertyChanged(nameof(TicketStatus));
            }
        }
    }
    public VehicleStatus? VehicleStatus
    {
        get => _vehicleStatus;
        set
        {
            if (_vehicleStatus != value)
            {
                _vehicleStatus = value;
                OnPropertyChanged(nameof(VehicleStatus));
            }
        }
    }

    public decimal? PriceFrom
    {
        get => _priceFrom;
        set
        {
            if (_priceFrom != value)
            {
                _priceFrom = value;
                OnPropertyChanged(nameof(PriceFrom));
            }
        }
    }

    public decimal? PriceTo
    {
        get => _priceTo;
        set
        {
            if (_priceTo != value)
            {
                _priceTo = value;
                OnPropertyChanged(nameof(PriceTo));
            }
        }
    }

    public string? EntryRegistrar
    {
        get => _entryRegistrar;
        set
        {
            if (_entryRegistrar != value)
            {
                _entryRegistrar = value;
                OnPropertyChanged(nameof(EntryRegistrar));
            }
        }
    }

    public string? ExitRegistrar
    {
        get => _exitRegistrar;
        set
        {
            if (_exitRegistrar != value)
            {
                _exitRegistrar = value;
                OnPropertyChanged(nameof(ExitRegistrar));
            }
        }
    }



    public bool? HasDiscrepancy
    {
        get => _hasDiscrepancy;
        set
        {
            if (_hasDiscrepancy != value)
            {
                _hasDiscrepancy = value;
                OnPropertyChanged(nameof(HasDiscrepancy));
            }
        }
    }
    public int? Discount
    {
        get => _discount;
        set
        {
            if (_discount != value)
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
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
                OnPropertyChanged(nameof(TotalPages)); // TotalPages depends on ItemsPerPage
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
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(TotalPages)); // TotalPages depends on TotalCount
            }
        }
    }
    public ObservableCollection<TicketsListViewModel> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
            }
        }
    }
    public int TotalPages => (int)Math.Ceiling((double)_totalCount / _itemsPerPage);

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
