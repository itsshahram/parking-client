using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class RolePageViewModel : INotifyPropertyChanged
{
    private ObservableCollection<ApplicationRole> _items = new();

    public ObservableCollection<ApplicationRole> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value ?? new ObservableCollection<ApplicationRole>();
                OnPropertyChanged(nameof(Items));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
