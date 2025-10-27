namespace Parking.App.Helpers;

public class DatabaseMonitorService
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private System.Threading.Timer? _timer;
    private bool _isReachable = true;

    public event Action? DatabaseLost;
    public event Action? DatabaseRestored;

    public DatabaseMonitorService(DbContextOptions<ApplicationDbContext> options)
    {
        _options = options;
    }

    public void StartMonitoring(int intervalSeconds = 5)
    {
        _timer = new System.Threading.Timer(async _ => await CheckDatabaseAsync(), null, 0, intervalSeconds * 1000);
    }

    public void StopMonitoring()
    {
        _timer?.Dispose();
    }

    private async Task CheckDatabaseAsync()
    {
        bool reachable;
        try
        {
            var connected = await DatabaseConnectionTester.TestConnectionAsync(Settings.Default.Application_DbHostAddress, Settings.Default.Application_DbUsername, Settings.Default.Application_DbPassword);
            reachable = connected.Item1;
        }
        catch
        {
            reachable = false;
        }

        if (reachable != _isReachable)
        {
            _isReachable = reachable;
            if (!_isReachable)
                DatabaseLost?.Invoke();
            else
                DatabaseRestored?.Invoke();
        }
    }

    public bool IsReachable => _isReachable;
}

