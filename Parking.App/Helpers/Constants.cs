namespace Parking.App.Helpers;

public static class Constants
{
    public static readonly string AppDataFolder =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parking.App");
    public static readonly string CredentialsPath = Path.Combine(AppDataFolder, "credentials.dat");
    public static readonly string KeyPath = Path.Combine(AppDataFolder, "aeskey.bin");
}
