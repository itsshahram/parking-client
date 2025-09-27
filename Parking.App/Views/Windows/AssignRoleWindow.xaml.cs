using System.ComponentModel;

namespace Parking.App.Views.Windows;

public partial class AssignRoleWindow : FluentWindow, INotifyPropertyChanged
{
    private readonly Guid _userId;
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    private ObservableCollection<AssignRoleItem> _roles = new();
    public ObservableCollection<AssignRoleItem> Roles
    {
        get => _roles;
        set { _roles = value; OnPropertyChanged(nameof(Roles)); }
    }

    private ObservableCollection<AssignPermissionItem> _permissions = new();
    public ObservableCollection<AssignPermissionItem> Permissions
    {
        get => _permissions;
        set { _permissions = value; OnPropertyChanged(nameof(Permissions)); }
    }

    private AssignRoleItem? _selectedRole;
    public AssignRoleItem? SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (_selectedRole != value)
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                _ = LoadPermissionsAsync();
            }
        }
    }

    public AssignRoleWindow(Guid userId)
    {
        InitializeComponent();
        _roleService = App.GetService<IRoleService>();
        _userService = App.GetService<IUserService>();
        _userId = userId;
        DataContext = this;
        _ = LoadRolesAsync();
    }

    private async Task LoadRolesAsync()
    {
        var roles = await _roleService.GetRoles();
        var userRole = await _userService.GetUserRole(_userId);

        Roles.Clear();
        foreach (var r in roles)
        {
            Roles.Add(new AssignRoleItem
            {
                Id = r.Id,
                FaName = r.FaName,
                IsSelected = userRole?.RoleId == r.Id
            });
        }

        SelectedRole = Roles.FirstOrDefault(r => r.IsSelected) ?? Roles.FirstOrDefault();
    }

    private async Task LoadPermissionsAsync()
    {
        if (SelectedRole == null) return;

        var allPermissions = await _roleService.GetPermissionsAsync();
        var rolePermissions = await _roleService.GetRolePermissions(SelectedRole.Id);
        var userPermissions = await _roleService.GetUserPermissions(SelectedRole.Id, _userId);

        var rolePermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToHashSet();

        Permissions.Clear();
        foreach (var p in allPermissions)
        {
            var item = new AssignPermissionItem(p.Id, p.Name, p.FaName)
            {
                IsSelected = rolePermissionIds.Contains(p.Id) || userPermissionIds.Contains(p.Id),
                IsEnabled = !rolePermissionIds.Contains(p.Id)
            };
            item.PropertyChanged += Permission_PropertyChanged;
            Permissions.Add(item);
        }
    }

    private void Permission_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedRole == null) return;

        await _roleService.AssignRoleToUser(_userId, SelectedRole.Id);

        var userPermissionIds = Permissions
                                .Where(p => p.IsSelected && p.IsEnabled)
                                .Select(p => p.Id);
        await _roleService.AddToUserPermissions(SelectedRole.Id, _userId, userPermissionIds);

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

public class AssignRoleItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string FaName { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class AssignPermissionItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FaName { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public bool IsEnabled { get; set; } = true;

    public AssignPermissionItem(Guid id, string name, string faName)
    {
        Id = id;
        Name = name;
        FaName = faName;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
