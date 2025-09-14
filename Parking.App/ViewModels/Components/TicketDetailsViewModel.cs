using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Components;

public class TicketDetailsViewModel : INotifyPropertyChanged
{
    private string? _plateText;
    private string? _entryTime;
    private string? _vehicleSegmentPrice;
    public string? PlateText
    {
        get => _plateText;
        set { _plateText = value; OnPropertyChanged(nameof(PlateText)); }
    }

    public string? EntryTime
    {
        get => _entryTime;
        set { _entryTime = value; OnPropertyChanged(nameof(EntryTime)); }
    }

    public string? VehicleSegmentPrice
    {
        get => _vehicleSegmentPrice;
        set { _vehicleSegmentPrice = value; OnPropertyChanged(nameof(VehicleSegmentPrice)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
