using System.ComponentModel;

namespace Parking.App.Views.Windows
{
    public enum PermissionMode
    {
        Role,
        User
    }

    public partial class ManagePermissionsWindow : FluentWindow, INotifyPropertyChanged
    {
        private readonly Guid _roleId;
        private readonly Guid? _userId; 
        private readonly PermissionMode _mode;

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

        public ManagePermissionsWindow(Guid roleId, PermissionMode mode, Guid? userId = null)
        {
            InitializeComponent();
            _roleService = App.GetService<IRoleService>();
            _roleId = roleId;
            _mode = mode;
            _userId = userId;
            DataContext = this;
            _ = LoadPermissionAsync();
        }

        private async Task LoadPermissionAsync()
        {
            var allPermissions = await _roleService.GetPermissionsAsync();
            HashSet<Guid> assignedPermissionIds;

            if (_mode == PermissionMode.Role)
            {
                var rolePermissions = await _roleService.GetRolePermissions(_roleId);
                assignedPermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            }
            else
            {
                if (_userId == null)
                    throw new InvalidOperationException("UserId is required for User mode");

                var userPermissions = await _roleService.GetUserPermissions(_roleId, _userId.Value);
                assignedPermissionIds = userPermissions.Select(up => up.PermissionId).ToHashSet();
            }

            Permissions.Clear();
            foreach (var p in allPermissions.Select(x => new PermissionItem(x.Id, x.Name, x.FaName)))
            {
                p.IsSelected = assignedPermissionIds.Contains(p.Id);
                p.PropertyChanged += Permission_PropertyChanged;
                Permissions.Add(p);
            }

            _areAllPermissionsSelected = Permissions.All(p => p.IsSelected);
            OnPropertyChanged(nameof(AreAllPermissionsSelected));
        }

        private void Permission_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PermissionItem.IsSelected))
            {
                _areAllPermissionsSelected = Permissions.All(p => p.IsSelected);
                OnPropertyChanged(nameof(AreAllPermissionsSelected));
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = Permissions.Where(p => p.IsSelected).Select(p => p.Id);

            if (_mode == PermissionMode.Role)
            {
                await _roleService.AddToRolePermission(_roleId, selectedIds);
            }
            else if (_mode == PermissionMode.User && _userId.HasValue)
            {
                await _roleService.AddToUserPermissions(_roleId, _userId.Value, selectedIds);
            }

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
        public Guid Id { get; set; }
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

        public PermissionItem(Guid id, string name, string faName)
        {
            Id = id;
            Name = name;
            FaName = faName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
