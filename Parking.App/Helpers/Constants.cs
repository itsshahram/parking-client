namespace Parking.App.Helpers;

public static class Constants
{
    public static readonly string AppDataFolder =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parking.App");
    public static readonly string CredentialsPath = Path.Combine(AppDataFolder, "credentials.dat");
    public static readonly string KeyPath = Path.Combine(AppDataFolder, "aeskey.bin");
    public static readonly string SecKeyPath = Path.Combine(AppDataFolder, "sec.key");
}

public static class DatabaseCredentials
{
    public static string Host { get; set; } = "";
    public static string DbName { get; set; } = "";
    public static string DbUserName { get; set; } = "";
    public static string DbPassword { get; set; } = "";

    public static bool IsLoaded =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(DbName) &&
        !string.IsNullOrWhiteSpace(DbUserName) &&
        !string.IsNullOrWhiteSpace(DbPassword);
}
