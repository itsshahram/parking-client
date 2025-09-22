using System.ComponentModel;

namespace Parking.App.Utilities;

public class PermissionManager : INotifyPropertyChanged
{
    private static PermissionManager _instance;
    public static PermissionManager Instance => _instance ??= new PermissionManager();

    private List<string> _userPermissions = new();
    public IReadOnlyList<string> UserPermissions => _userPermissions.AsReadOnly();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetUserPermissions(IEnumerable<string> permissions)
    {
        _userPermissions = permissions?.ToList() ?? new List<string>();
        OnPropertyChanged(nameof(UserPermissions));
    }

    public bool HasPermission(string permission) => _userPermissions.Contains(permission);

    private void OnPropertyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

