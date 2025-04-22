using Parking.App.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        set => _roleName = value;
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

}



