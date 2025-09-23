using Parking.App.Attributes;
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
    public ICommand ManagePermissionCommand { get; }

    public RolePageViewModel()
    {
        ManagePermissionCommand = new RelayCommand<ApplicationRole>(OnManagePermission);
    }
    [RequiresPermission("ManagePermission", "مدیریت دسترسی")]
    private void OnManagePermission(ApplicationRole role)
    {
        if (role == null) return;

        var managePermissionsWindow = new ManagePermissionsWindow(role.Id, PermissionMode.Role)
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
