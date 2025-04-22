using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Net;
using ZXing.Aztec.Internal;

namespace Parking.App.Helpers;

public static class LogHelper
{
    public static void LogDetectedPlate(string plateIage, string carImage, string enLicensePlate, string message, Microsoft.Extensions.Logging.ILogger _logger)
    {
        using (LogContext.PushProperty("UserName", $"{TokenStore.Username}"))
        using (LogContext.PushProperty("level", $"DetectPlate"))
        using (LogContext.PushProperty("TransactionId", Guid.NewGuid().ToString()))
        using (LogContext.PushProperty("MachineName", Environment.MachineName))
        using (LogContext.PushProperty("IP_Address", GetLocalIPAddress()))
        using (LogContext.PushProperty("PlateImage", plateIage))
        using (LogContext.PushProperty("CarImage", carImage))
        using (LogContext.PushProperty("LicensePlate", enLicensePlate))
        {
            Serilog.Log.Logger.Write(DetectPlate, message);

        }
    }
    public static void LogUserEvent(string message, Microsoft.Extensions.Logging.ILogger _logger)
    {
        using (LogContext.PushProperty("UserName", $"{TokenStore.Username}"))
        using (LogContext.PushProperty("level", $"DetectPlate"))
        using (LogContext.PushProperty("TransactionId", Guid.NewGuid().ToString()))
        using (LogContext.PushProperty("MachineName", Environment.MachineName))
        using (LogContext.PushProperty("IP_Address", GetLocalIPAddress()))
        {
            Serilog.Log.Logger.Write(DetectPlate, message);

        }
    }

    //public static readonly LogLevel DetectPlate = (LogLevel)60;
    public static readonly LogEventLevel DetectPlate = (LogEventLevel)60;
    public static readonly LogEventLevel UserEvent = (LogEventLevel)60;
    private static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddr in host.AddressList)
            {
                if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipAddr.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("خطا در دریافت IP: " + ex.Message);
        }
        return "Unknown";
    }
}
