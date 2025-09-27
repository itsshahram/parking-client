using Parking.App.Models.Dto.User;
using System.ComponentModel;

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
    public ICommand ManagePermissionCommand { get; }
    public ICommand AssignRoleCommand { get; }


    public UsersListPageViewModel()
    {
        ManagePermissionCommand = new RelayCommand<UserListItemModel>(OnManagePermission);
        AssignRoleCommand = new RelayCommand<UserListItemModel>(OnAssignRole);
    }

    private void OnAssignRole(UserListItemModel user)
    {
        if (user == null) return;

        var assignRoleWindow = new AssignRoleWindow(user.Id)
        {
            Owner = App.Current.MainWindow
        };

        if (assignRoleWindow.ShowDialog() == true)
        {

        }
    }
    private void OnManagePermission(UserListItemModel user)
    {
        if (user == null) return;

        var managePermissionsWindow = new ManagePermissionsWindow(user.RoleId.Value, PermissionMode.User, user.Id)
        {
            Owner = App.Current.MainWindow
        };

        if (managePermissionsWindow.ShowDialog() == true)
        {

        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
