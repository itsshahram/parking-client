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
    private ObservableCollection<UserListItemModel> _items = new();

    public ObservableCollection<UserListItemModel> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value ?? new ObservableCollection<UserListItemModel>(); 
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
