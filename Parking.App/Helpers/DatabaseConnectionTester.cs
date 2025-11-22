using Microsoft.Data.SqlClient;


namespace Parking.App.Helpers;
public static class DatabaseConnectionTester
{
    public static async Task<(bool, string)> TestConnectionAsync(string server, string username, string password)
    {
        string connectionString = $"Server={server};User Id={username};Password={password};Encrypt=true;TrustServerCertificate=true;Connect Timeout=2;";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Connection successful.");
                return (true, "");
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Connection failed: {ex.Message} \nStackTrace: {ex.StackTrace}";
            Console.WriteLine(errorMessage);

            File.AppendAllText("connection_errors.log", errorMessage + Environment.NewLine);

            return (false, errorMessage);
        }
    }
    public static async Task<(bool, string)> TestConnectionAsync(string connectionString)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Connection successful.");
                return (true, "");

            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Connection failed: {ex.Message} \nStackTrace: {ex.StackTrace}";
            Console.WriteLine(errorMessage);
            File.AppendAllText("connection_errors.log", errorMessage + Environment.NewLine);

            return (false, errorMessage);
        }
    }
}