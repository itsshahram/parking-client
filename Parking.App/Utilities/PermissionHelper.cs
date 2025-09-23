namespace Parking.App.Utilities;

public static class PermissionHelper
{
    public static bool CheckUserPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        return PermissionManager.Instance.HasPermission(permission);
    }
}