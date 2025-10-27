using System.Diagnostics;

namespace Parking.App.Helpers;

public static class AppInfoHelper
{
    public static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var infoVer = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        var version = "Unknown";

        if (!string.IsNullOrEmpty(infoVer))
            version = infoVer.Split('-', '+')[0];

        return version;
    }

    public static string GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var filePath = assembly.Location;
        var fileInfo = FileVersionInfo.GetVersionInfo(filePath);

        return File.GetLastWriteTime(filePath).ToShamsi();
    }
}
