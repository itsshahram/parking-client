using Parking.App.Models.Dto.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Pages;

public class UsersListPageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<UserListItemModel> _items;
    public ObservableCollection<UserListItemModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged(nameof(Items));
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
