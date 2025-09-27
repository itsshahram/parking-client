using Parking.Domain.Entities;

namespace Parking.App.Helpers;

public static class TokenStore
{
    private static bool _serverStatus;
    private static string _bearerToken;
    private static string _baseUrl;
    private static int _parkingLotId;
    private static Guid _userId;
    private static string _fullName;
    private static DateTime _expirationDateTime;
    private static string _userName;
    private static string _roleName;
    private static bool _isAuthenticated;

    public static event EventHandler RoleChanged;

    private static List<string> _permissions = new List<string>();
    public static IReadOnlyList<string> Permissions => _permissions.AsReadOnly();

    public static void SetPermissions(IEnumerable<string> permissions)
    {
        _permissions = permissions?.ToList() ?? new List<string>();
        PermissionManager.Instance.SetUserPermissions(_permissions);
    }
    public static void DeletePermissions()
    {
        _permissions = new List<string>();
        PermissionManager.Instance.SetUserPermissions(_permissions);
    }

    public static void LoadUserPermissions(string roleName, List<RolePermission> permissions)
    {
        SetPermissions(permissions.Select(x => x.Permission.Name));
    }

    public static bool ServerStatus
    {
        get => _serverStatus;
        set => _serverStatus = value;
    }
    public static string Username
    {
        get => _userName;
        set => _userName = value;
    }
    public static string RoleName
    {
        get => _roleName;
        set
        {
            if (_roleName != value)
            {
                _roleName = value;
                RoleChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    public static DateTime ExpirationDateTime
    {
        get => _expirationDateTime;
        set => _expirationDateTime = value;
    }
    public static string BearerToken
    {
        get => _bearerToken;
        set => _bearerToken = value;
    }
    public static string BaseUrl
    {
        get => _baseUrl;
        set => _baseUrl = value;
    }
    public static int ParkingLotId
    {
        get => _parkingLotId;
        set => _parkingLotId = value;
    }
    public static Guid UserId
    {
        get => _userId;
        set => _userId = value;
    }
    public static String FullName
    {
        get => _fullName;
        set => _fullName = value;
    }
    public static bool IsAuthenticated
    {
        get => !string.IsNullOrWhiteSpace(_fullName);
        set { }
    }


    public static void Clear()
    {
        _serverStatus = false;
        _bearerToken = null;
        _baseUrl = null;
        _parkingLotId = 0;
        _userId = Guid.Empty;
        _fullName = null;
        _expirationDateTime = DateTime.MinValue;
        _userName = null;
        _roleName = null;
        _permissions.Clear();
    }
}



