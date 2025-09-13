namespace Parking.App.Utilities;

public static class PermissionHelper
{
    public static bool CheckUserPermission(string enRoleName, string action)
    {
        bool result = false;
        if (enRoleName == null)
        {
            return false;
        }
        if (action == "UserManagement")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }
        if (action == "ApplicationSettings")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }
        if (action == "AddCards")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }
        if (action == "ForceExitRequest")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }

        if (action == "SyncAllDeviceTickets")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }

        if (action == "CustomAmouontPayment")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }
        if (action == "FullReport")
        {
            result = enRoleName.ToUpper() switch
            {
                "PARKINGMANAGER" => true,
                "PARKINGAGENT" => false,
                _ => false
            };
        }
        return result;
    }
}
