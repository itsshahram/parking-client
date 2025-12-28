using Parking.App.Models.Dto.User;
using Parking.App.Services.Interfaces;
using System.ComponentModel;
using Parking.Domain.Enums;

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

    private async Task ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                    ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                    ms.Title = title;
                    ms.Content = message;
                    ms.IsPrimaryButtonEnabled = false;
                    ms.IsSecondaryButtonEnabled = false;
                    ms.CloseButtonText = "متوجه شدم";
                    await ms.ShowDialogAsync();
                });
            }
        }
        catch
        {
            System.Windows.MessageBox.Show(message, title);
        }
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

        if (editUserWindow.ShowDialog() != true)
            return;

        var existingUser = _userService.GetUserById(user.Id);
        if (existingUser == null)
        {
            await ShowMessage("خطا", "کاربر یافت نشد");
            return;
        }

        existingUser.Firstname = editUserWindow.ViewModel.FirstName;
        existingUser.Lastname = editUserWindow.ViewModel.LastName;
        existingUser.UserName = editUserWindow.ViewModel.Username;

        var status = await _userService.UpdateUserAsync(existingUser);

        switch (status)
        {
            case UserServiceStatus.Success:
                await ShowMessage("موفق", "اطلاعات کاربر با موفقیت ویرایش شد");
                break;

            case UserServiceStatus.UserNameExist:
                await ShowMessage("خطا", "نام کاربری قبلاً استفاده شده است");
                break;

            case UserServiceStatus.NotFound:
                await ShowMessage("خطا", "کاربر یافت نشد");
                break;

            case UserServiceStatus.Failed:
            default:
                await ShowMessage("خطا", "خطا در ویرایش اطلاعات");
                break;
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
