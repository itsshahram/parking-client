using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Pages;

using System.ComponentModel;

public class DataPageViewModel : INotifyPropertyChanged
{
    private string _parkingName;
    private int _totalCards;
    private int _totalDiscountedCards;
    private int _totalSpaces;
    private int _occupiedSpaces;

    // نام پارکینگ
    public string ParkingName
    {
        get => _parkingName;
        set
        {
            _parkingName = value;
            OnPropertyChanged(nameof(ParkingName));
        }
    }

    // تعداد کارت‌های تعریف‌شده
    public int TotalCards
    {
        get => _totalCards;
        set
        {
            _totalCards = value;
            OnPropertyChanged(nameof(TotalCards));
        }
    }

    // تعداد کارت‌های تعریف‌شده
    public int TotalDiscountedCards
    {
        get => _totalDiscountedCards;
        set
        {
            _totalDiscountedCards = value;
            OnPropertyChanged(nameof(TotalDiscountedCards));
        }
    }


    // تعداد کل فضای تعریف‌شده
    public int TotalSpaces
    {
        get => _totalSpaces;
        set
        {
            _totalSpaces = value;
            OnPropertyChanged(nameof(TotalSpaces));
            OnPropertyChanged(nameof(AvailableSpaces)); 
        }
    }

    // تعداد فضای پر شده
    public int OccupiedSpaces
    {
        get => _occupiedSpaces;
        set
        {
            _occupiedSpaces = value;
            OnPropertyChanged(nameof(OccupiedSpaces));
            OnPropertyChanged(nameof(AvailableSpaces));
        }
    }


    public int AvailableSpaces => TotalSpaces - OccupiedSpaces;


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}