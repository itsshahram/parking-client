using Parking.App.Models.Dto.User;
using Parking.App.Services.Interfaces;
using System.ComponentModel;

namespace Parking.App.ViewModels.Pages;

public class UsersListPageViewModel : INotifyPropertyChanged
{
    private readonly IUserService _userService;
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
    public ICommand EditUserCommand { get; }


    public UsersListPageViewModel(IUserService userService)
    {
        _userService = userService;
        ManagePermissionCommand = new RelayCommand<UserListItemModel>(OnManagePermission);
        AssignRoleCommand = new RelayCommand<UserListItemModel>(OnAssignRole);
        EditUserCommand = new RelayCommand<UserListItemModel>(OnEditUser);
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

    private async void OnEditUser(UserListItemModel user)
    {
        if (user == null) return;

        var editUserWindow = new EditUserWindow(user)
        {
            Owner = App.Current.MainWindow
        };

        if (editUserWindow.ShowDialog() == true)
        {
            var existingUser = _userService.GetUserById(user.Id);
            if (existingUser != null)
            {
                // Update fields
                existingUser.Firstname = editUserWindow.ViewModel.FirstName;
                existingUser.Lastname = editUserWindow.ViewModel.LastName;
                existingUser.UserName = editUserWindow.ViewModel.Username;

                // Save to database
                var success = await _userService.UpdateUserAsync(existingUser);
                if (success)
                {
                    // Update the UI model
                    user.Firstname = editUserWindow.ViewModel.FirstName;
                    user.Lastname = editUserWindow.ViewModel.LastName;
                    user.UserName = editUserWindow.ViewModel.Username;
                    user.Fullname = $"{editUserWindow.ViewModel.FirstName} {editUserWindow.ViewModel.LastName}";
                    OnPropertyChanged(nameof(Items)); 
                }
            }
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
