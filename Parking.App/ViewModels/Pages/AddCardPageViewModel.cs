using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Pages;

public class AddCardPageViewModel : INotifyPropertyChanged
{
    private IParkingService _parkingService;
    private string? _enLicensePlate { get; set; }
    public string? EnLicensePlate
    {
        get => _enLicensePlate;
        set
        {
            _enLicensePlate = value;
            OnPropertyChanged(nameof(EnLicensePlate));
        }
    }
    private string? _ownerFirstName { get; set; }
    public string? OwnerFirstName
    {
        get => _ownerFirstName;
        set
        {
            _ownerFirstName = value;
            OnPropertyChanged(nameof(OwnerFirstName));
        }
    }
    private string? _ownerLastName { get; set; }
    public string? OwnerLastName
    {
        get => _ownerLastName;
        set
        {
            _ownerLastName = value;
            OnPropertyChanged(nameof(OwnerLastName));
        }
    }
    private string? _ownerNationalCode { get; set; }
    public string? OwnerNationalCode
    {
        get => _ownerNationalCode;
        set
        {
            _ownerNationalCode = value;
            OnPropertyChanged(nameof(OwnerNationalCode));
        }
    }
    private string? _ownerAddress { get; set; }
    public string? OwnerAddress
    {
        get => _ownerAddress;
        set
        {
            _ownerAddress = value;
            OnPropertyChanged(nameof(OwnerAddress));
        }
    }
    private int _validityPeriod;
    public int ValidityPeriod
    {
        get => _validityPeriod;
        set
        {
            _validityPeriod = value;
            OnPropertyChanged(nameof(ValidityPeriod));
        }
    }
    private int _fixDiscount { get; set; }
    public int FixDiscount
    {
        get => _fixDiscount;
        set
        {
            _fixDiscount = value;
            OnPropertyChanged(nameof(FixDiscount));
        }
    }
    private int _percentDiscount { get; set; }
    public int PercentDiscount
    {
        get => _percentDiscount;
        set
        {
            _percentDiscount = value;
            OnPropertyChanged(nameof(PercentDiscount));
        }
    }
    private Guid? _licensePlateGroupId { get; set; }
    public Guid? LicensePlateGroupId
    {
        get => _licensePlateGroupId;
        set
        {
            _licensePlateGroupId = value;
            OnPropertyChanged(nameof(LicensePlateGroupId));
        }
    }
    private int? _vehicleSegmentId { get; set; }
    public int? VehicleSegmentId
    {
        get => _vehicleSegmentId;
        set
        {
            _vehicleSegmentId = value;
            OnPropertyChanged(nameof(VehicleSegmentId));
        }
    }
    private long? _credit { get; set; }
    public long? Credit
    {
        get => _credit;
        set
        {
            _credit = value;
            OnPropertyChanged(nameof(Credit));
        }
    }
    private long? _cardSerialNo { get; set; }
    public long? CardSerialNo
    {
        get => _cardSerialNo;
        set
        {
            _cardSerialNo = value;
            OnPropertyChanged(nameof(CardSerialNo));
        }
    }
    private bool _isGuest { get; set; }
    public bool IsGuest
    {
        get => _isGuest;
        set
        {
            _isGuest = value;
            OnPropertyChanged(nameof(IsGuest));
        }
    }
    private bool _isActive { get; set; }
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    private VehicleSegmentModel _selectedSegment;
    public VehicleSegmentModel SelectedSegment
    {
        get => _selectedSegment;
        set
        {
            _selectedSegment = value;
            OnPropertyChanged(nameof(SelectedSegment));
        }
    }
    public List<VehicleSegmentModel> VehicleSegments { get; set; } = new();


    public AddCardPageViewModel()
    {
        _parkingService = App.GetService<IParkingService>();
        LoadVehicleSegments();
    }
    private async void LoadVehicleSegments()
    {
        VehicleSegments = _parkingService.GetVehicleSegments();
        OnPropertyChanged(nameof(VehicleSegments));
    }




    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
