using System.ComponentModel;
using System.Threading.Tasks;

namespace Parking.App.Views.Windows;

public partial class ManagePermissionsWindow : FluentWindow, INotifyPropertyChanged
{
    private bool _areAllPermissionsSelected;
    public bool AreAllPermissionsSelected
    {
        get => _areAllPermissionsSelected;
        set
        {
            if (_areAllPermissionsSelected != value)
            {
                _areAllPermissionsSelected = value;
                OnPropertyChanged(nameof(AreAllPermissionsSelected));

                foreach (var p in Permissions)
                    p.IsSelected = value;
            }
        }
    }

    private ObservableCollection<PermissionItem> _permissions = new();
    public ObservableCollection<PermissionItem> Permissions
    {
        get => _permissions;
        set
        {
            if (_permissions != value)
            {
                _permissions = value;
                OnPropertyChanged(nameof(Permissions));
            }
        }
    }
    private readonly IRoleService _roleService;


    public ManagePermissionsWindow()
    {
        InitializeComponent();
        _roleService = App.GetService<IRoleService>();
        DataContext = this;
        _ = LoadPermissionAsync();
    }

    private async Task LoadPermissionAsync()
    {
        var data = await _roleService.GetPermissionsAsync();

        Permissions.Clear();
        foreach (var p in data.Select(x => new PermissionItem(x.Name, x.FaName)))
        {
            p.PropertyChanged += Permission_PropertyChanged;
            Permissions.Add(p);
        }
    }

    private void Permission_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PermissionItem.IsSelected))
        {
            _areAllPermissionsSelected = Permissions.All(p => p.IsSelected);
            OnPropertyChanged(nameof(AreAllPermissionsSelected));
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class PermissionItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string FaName { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public PermissionItem(string name, string faName)
    {
        Name = name;
        FaName = faName;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
