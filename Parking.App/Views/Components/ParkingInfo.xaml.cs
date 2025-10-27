
using static Parking.App.Helpers.AppInfoHelper;

namespace Parking.App.Views.Components;

/// <summary>
/// Interaction logic for ParkingInfo.xaml
/// </summary>
public partial class ParkingInfo : UserControl
{
    private readonly IParkingService _parkingService;
    private readonly ILogger<ParkingInfo> _logger;
    private readonly ISynchronizationService _synchronizationService;
    private DispatcherTimer _timeTimer;
    private DispatcherTimer _serverStatusTimer;
    private DispatcherTimer _dbStatusTimer;

    public ParkingInfo()
    {
        _parkingService = App.GetService<IParkingService>();
        _synchronizationService = App.GetService<ISynchronizationService>();
        _logger = App.GetService<ILogger<ParkingInfo>>();

        ParkingLotInfoStore.ParkingInfo = _parkingService.GetParkingLotDetails().Result;

        InitializeComponent();

        // Initialize UI
        PakingNameText.Text = ParkingLotInfoStore.ParkingInfo?.Name;
        DateText.Text = DateTime.Now.ToLongShamsiString();
        TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        UserFullName.Text = TokenStore.FullName ?? "-----";

        // Timers
        InitializeTimeTimer();
        InitializeServerStatusCheck();
        InitializeDatabaseStatusCheck();

        // Set initial server icon
        UpdateServerIcon(TokenStore.ServerStatus);

        VersionText.Text = GetVersion();
    }

    #region Time Timer
    private void InitializeTimeTimer()
    {
        _timeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timeTimer.Tick += TimeTimer_Tick;
        _timeTimer.Start();
    }

    private void TimeTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            DateText.Text = DateTime.Now.ToLongShamsiString();
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update time.");
        }
    }
    #endregion

    #region Server Status Timer
    private void InitializeServerStatusCheck()
    {
        _serverStatusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _serverStatusTimer.Tick += ServerStatusTimer_Tick;
        _serverStatusTimer.Start();
    }

    private void ServerStatusTimer_Tick(object sender, EventArgs e)
    {
        if (!Settings.Default.Application_Sync_Enable) return;

        UpdateServerIcon(TokenStore.ServerStatus);
    }

    private void UpdateServerIcon(bool isConnected)
    {
        if (isConnected)
        {
            UpdateIconAndColor(ServerStatusIcon, SymbolRegular.CloudCheckmark16,
                "SystemFillColorSuccessBrush", "ارتباط با سرور برقرار میباشد");
        }
        else
        {
            UpdateIconAndColor(ServerStatusIcon, SymbolRegular.CloudDismiss16,
                "SystemFillColorCriticalBrush", "ارتباط با سرور برقرار نمیباشد");
        }
    }
    #endregion

    #region Database Status Timer
    private void InitializeDatabaseStatusCheck()
    {
        _dbStatusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _dbStatusTimer.Tick += DatabaseStatusTimer_Tick;
        _dbStatusTimer.Start();
    }

    private async void DatabaseStatusTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            bool dbConnected = await CheckDatabaseConnectivityAsync();

            if (dbConnected)
            {
                UpdateDBStatusIconAndColor(SymbolRegular.Database16,
                    "SystemFillColorSuccessBrush", "ارتباط با پایگاه داده برقرار میباشد");
            }
            else
            {
                UpdateDBStatusIconAndColor(SymbolRegular.Database16,
                    "SystemFillColorCriticalBrush", "ارتباط با پایگاه داده برقرار نیست");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check database connectivity.");
        }
    }

    private async Task<bool> CheckDatabaseConnectivityAsync()
    {
        if (App.DatabaseMonitor != null)
            return App.DatabaseMonitor.IsReachable;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(Settings.Default.Application_DbHostAddress);

        try
        {
            using var context = new ApplicationDbContext(optionsBuilder.Options);
            var connectTask = context.Database.CanConnectAsync();
            var timeoutTask = Task.Delay(3000);
            var finishedTask = await Task.WhenAny(connectTask, timeoutTask);
            return finishedTask == connectTask && await connectTask;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region Helper Methods
    private void UpdateIconAndColor(SymbolIcon iconControl, SymbolRegular icon, string colorResource, string tooltip)
    {
        iconControl.Symbol = icon;
        iconControl.Foreground = (System.Windows.Media.Brush)Application.Current.Resources[colorResource];
        iconControl.ToolTip = tooltip;
    }

    private void UpdateDBStatusIconAndColor(SymbolRegular icon, string colorResource, string tooltip)
    {
        DbStatusIcon.Symbol = icon;
        DbStatusIcon.Foreground = (System.Windows.Media.Brush)Application.Current.Resources[colorResource];
        DbStatusIcon.ToolTip = tooltip;
    }
    #endregion
}
