using Coravel;
using Parking.App.Seeds;
using Parking.App.Views.Pages.LicensePlate;
using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities.User;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Net;
using Log = Serilog.Log;
using MessageBox = System.Windows.MessageBox;

namespace Parking.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += UnHandleException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.Error(e.Exception, "Unhandled Dispatcher (UI) exception occurred.");

        if (e.Exception is ArgumentException or InvalidOperationException)
        {
            e.Handled = true;
            return;
        }

        e.Handled = false;
    }

    private void UnHandleException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            _logger?.Error(ex, "Unhandled domain exception occurred.");
        else
            _logger?.Error("Unhandled domain exception: {0}", e.ExceptionObject);

        // برنامه را در حالت ناقص رها نکن؛ بهتره تمیز ببنده
        Environment.Exit(1);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.Error(e.Exception, "Unobserved Task exception occurred.");
        e.SetObserved();
    }

    public IConfiguration? Configuration { get; private set; }

    public Serilog.ILogger _logger;
    public static CancellationTokenSource GlobalCancellationTokenSource { get; private set; } = new();
    public static DatabaseMonitorService DatabaseMonitor { get; private set; }

    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            if (Settings.Default.Application_DbActiveStatus)
            {
                var connectionString =
                    $"Server={Settings.Default.Application_DbHostAddress};" +
                    $"Database={Settings.Default.Application_DbName};" +
                    $"User Id={Settings.Default.Application_DbUsername};" +
                    $"Password={Settings.Default.Application_DbPassword};" +
                    $"TrustServerCertificate=true;MultipleActiveResultSets=True;";


                services.AddDbContextFactory<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });

                services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();


                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                //  Business Services
                services.AddScoped<IParkingService, ParkingService>();
                services.AddScoped<IRoleService, RoleService>();
                services.AddScoped<ISynchronizationService, SynchronizationService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<ITicketQueueService, TicketQueueService>();
                services.AddScoped<IThemeService, ThemeService>();


                services.AddScheduler();
                services.AddTransient<BackgroundTask>();


                services.AddScoped<MainWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<DatabaseErrorWindow>();
                services.AddTransient<AddUserWindow>();
                services.AddTransient<TicketDetailsWindow>();
                services.AddTransient<CustomAmountPaymentModalWindow>();


                services.AddScoped<MainPage>();
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<SettingsPageViewModel>();
                services.AddScoped<LicensePlateGroupPage>();
                services.AddScoped<LicensePlateTabsPage>();
                services.AddScoped<LicensePlateGroupViewModel>();
                services.AddScoped<AddCardPage>();
                services.AddScoped<AddCardPageViewModel>();
                services.AddScoped<UsersListPage>();
                services.AddScoped<UsersListPageViewModel>();
                services.AddScoped<AddCardHistoryPageViewModel>();


                services.AddScoped<UserManager<ApplicationUser>>();
                services.AddScoped<RoleManager<IdentityRole<Guid>>>();


                services.AddHttpClient();
            }
            else
            {
                services.AddScoped<ConfigDatabaseWindow>();
            }
        })
        .UseSerilog()
        .Build();

    public static T? GetService<T>() where T : class
        => _host.Services.GetService(typeof(T)) as T;

    public MainWindow? mainWindow { get; private set; }



    private async void OnStartup(object sender, StartupEventArgs e)
    {
        if (Settings.Default.IsFirstRun)
        {
            Settings.Default.Upgrade();
            Settings.Default.IsFirstRun = false;
            Settings.Default.Save();
        }

        ConfigureLogging();
        Log.Information("Application Started.");

        if (!Settings.Default.Application_DbActiveStatus)
        {
            var dbWindow = _host.Services.GetRequiredService<ConfigDatabaseWindow>();
            dbWindow.Show();
            return;
        }

        if (!SingleInstanceApp.IsFirstInstance())
        {
            SingleInstanceApp.ActivatePreviousInstance();
            Shutdown();
            return;
        }

        var connectionString = BuildConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        try
        {
            bool canConnect = await CanConnectToDatabaseAsync();
            if (!canConnect)
            {
                ShowDatabaseErrorWindow();
                return;
            }

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                await context.Database.MigrateAsync();
            }

            DatabaseMonitor = new DatabaseMonitorService(optionsBuilder.Options);
            DatabaseMonitor.DatabaseLost += () =>
                Dispatcher.BeginInvoke(ShowDatabaseErrorWindow);
            DatabaseMonitor.StartMonitoring();

            await SyncPermissionsWithDatabase(optionsBuilder.Options);

            _host.Start();

            var login = _host.Services.GetRequiredService<LoginWindow>();
            login.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    private static string BuildConnectionString()
    {
        return $"Server={Settings.Default.Application_DbHostAddress};" +
               $"Database={Settings.Default.Application_DbName};" +
               $"User Id={Settings.Default.Application_DbUsername};" +
               $"Password={Settings.Default.Application_DbPassword};" +
               $"TrustServerCertificate=true;MultipleActiveResultSets=True;";
    }


    public void ShowMainWindow()
    {
        if (mainWindow == null)
        {
            mainWindow = _host.Services.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
        }

        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        mainWindow.Activate();
    }

    public void CloseMainWindow()
    {
        if (mainWindow != null)
        {
            mainWindow.Close();
            mainWindow = null;
        }
    }

    private static bool _dbErrorWindowOpen = false;

    private static void ShowDatabaseErrorWindow()
    {
        if (_dbErrorWindowOpen) return;

        _dbErrorWindowOpen = true;
        try
        {
            var dbWindow = new DatabaseErrorWindow(Settings.Default.Application_DbHostAddress)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (Application.Current.MainWindow is not null
                && Application.Current.MainWindow != dbWindow
                && Application.Current.MainWindow is not LoginWindow)
            {
                dbWindow.Owner = Application.Current.MainWindow;
                dbWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            dbWindow.ShowDialog();
        }
        finally
        {
            _dbErrorWindowOpen = false;
        }
    }

    private static async Task<bool> CanConnectToDatabaseAsync()
    {
        try
        {
            var connected = await DatabaseConnectionTester.TestConnectionAsync(
                Settings.Default.Application_DbHostAddress,
                Settings.Default.Application_DbUsername,
                Settings.Default.Application_DbPassword);
            return connected.Item1;
        }
        catch
        {
            return false;
        }
    }

    private void ConfigureLogging()
    {
        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "log.txt");
        var crashFilePath = Path.Combine(logDirectory, "crash-log.txt");

        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("IP_Address", GetLocalIPAddress())

            .MinimumLevel.Information()

            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Infinite,
                shared: true,
                retainedFileCountLimit: null,
                rollOnFileSizeLimit: false,

                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )

            .WriteTo.File(
                crashFilePath,
                rollingInterval: RollingInterval.Infinite,
                shared: true,
                retainedFileCountLimit: null,
                rollOnFileSizeLimit: false,

                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );

        if (Settings.Default.Application_Logging_In_Elastic)
        {
            loggerConfig.WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(Settings.Default.Application_Logs_Elastic_Server))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Information,
                    ModifyConnectionSettings = x =>
                        x.BasicAuthentication(
                            Settings.Default.Application_Logs_Elastic_Username,
                            Settings.Default.Application_Logs_Elastic_Pass
                        )
                });
        }

        Log.Logger = loggerConfig.CreateLogger();
        _logger = Log.Logger;
        _logger.Information("Logger configured successfully.");
    }


    private async Task SyncPermissionsWithDatabase(DbContextOptions<ApplicationDbContext> options)
    {
        var applicationRole = GetService<RoleManager<ApplicationRole>>();
        await PermissionSeeder.SeedPermissionsAsync(options, applicationRole);
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        SingleInstanceApp.Cleanup();
    }

    private static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get IP: " + ex.Message);
        }
        return "Unknown";
    }
}
