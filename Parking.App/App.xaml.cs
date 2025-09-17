using Coravel;
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
    public IConfiguration? Configuration { get; private set; }
    public static CancellationTokenSource GlobalCancellationTokenSource { get; private set; } = new CancellationTokenSource();
    public static DatabaseMonitorService DatabaseMonitor { get; private set; }

    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            if (Settings.Default.Application_DbActiveStatus)
            {
                var connectionString = $"Server={Settings.Default.Application_DbHostAddress};Database={Settings.Default.Application_DbName};User Id={Settings.Default.Application_DbUsername};Password={Settings.Default.Application_DbPassword};TrustServerCertificate=true;MultipleActiveResultSets=True;";
                services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
                services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

                services.AddHttpClient();

                services.AddTransient<IUnitOfWork, UnitOfWork>();

                services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

                services.AddTransient<IParkingService, ParkingService>();
                services.AddTransient<ISynchronizationService, SynchronizationService>();
                services.AddTransient<IUserService, UserService>();
                services.AddTransient<ITicketQueueService, TicketQueueService>();

                services.AddScoped<IThemeService, ThemeService>();

                services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

                services.AddScheduler();
                services.AddTransient<BackgroundTask>();
                services.AddScoped<MainWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<DatabaseErrorWindow>();
                services.AddScoped<MainPage>();
                services.AddScoped<MainWindowViewModel>();
                services.AddTransient<TicketDetailsWindow>();
                services.AddTransient<CustomAmountPaymentModalWindow>();
                services.AddScoped<SettingsPageViewModel>();
                services.AddScoped<LicensePlateGroupPage>();
                services.AddScoped<LicensePlateGroupViewModel>();
                services.AddScoped<AddCardPage>();
                services.AddScoped<AddCardPageViewModel>();
                services.AddScoped<UserManager<ApplicationUser>>();
                services.AddScoped<RoleManager<IdentityRole<Guid>>>();



                services.AddScoped<ChangePasswordWindow>();


                services.AddScoped<UsersListPage>();
                services.AddScoped<UsersListPageViewModel>();
                services.AddScoped<AddCardHistoryPageViewModel>();
            }
            else
            {
                services.AddScoped<ConfigDatabaseWindow>();
            }


        }).UseSerilog().Build();
    public static T? GetService<T>()
    where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }


    public MainWindow? mainWindow { get; private set; }
    static string GetLocalIPAddress()
    {
        string ip = "Unknown";
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddr in host.AddressList)
            {
                if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = ipAddr.ToString();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("خطا در دریافت IP: " + ex.Message);
        }
        return ip;
    }

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
            bool canConnect = await CanConnectToDatabaseAsync(optionsBuilder.Options, TimeSpan.FromSeconds(3));
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
            DatabaseMonitor.DatabaseLost += () => Dispatcher.Invoke(ShowDatabaseErrorWindow);
            DatabaseMonitor.StartMonitoring();

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


    private static async Task<bool> CanConnectToDatabaseAsync(DbContextOptions<ApplicationDbContext> options, TimeSpan timeout)
    {
        try
        {
            using var context = new ApplicationDbContext(options);
            var connectTask = context.Database.CanConnectAsync();
            var timeoutTask = Task.Delay(timeout);

            var finishedTask = await Task.WhenAny(connectTask, timeoutTask);
            return finishedTask == connectTask && await connectTask;
        }
        catch
        {
            return false;
        }
    }

    private void ConfigureLogging()
    {
        if (Settings.Default.Application_Logging)
        {
            if (Settings.Default.Application_Logging_In_Elastic)
            {
                string password = Settings.Default.Application_Logs_Elastic_Pass;
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("IP_Address", GetLocalIPAddress())
                    .Enrich.WithProperty("MachineName", Settings.Default.Application_GatePCName)
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri($"{Settings.Default.Application_Logs_Elastic_Server}"))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = "logs-{0:yyyy.MM.dd}",
                        MinimumLogEventLevel = LogEventLevel.Information,
                        ModifyConnectionSettings = x =>
                            x.BasicAuthentication(Settings.Default.Application_Logs_Elastic_Username, password)
                    })
                    .WriteTo.File("logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: Settings.Default.Application_LoggingFileCount,
                        fileSizeLimitBytes: Settings.Default.Application_LoggingFileSize * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Error)
                    .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.File("logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: Settings.Default.Application_LoggingFileCount,
                        fileSizeLimitBytes: Settings.Default.Application_LoggingFileSize * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Error)
                    .CreateLogger();
            }
        }
    }

    private void MainWindow_Closed(object sender, ExitEventArgs e)
    {
        if (mainWindow != null)
        {
            Application.Current.MainWindow = mainWindow;
            mainWindow.Close();
        }

    }
    public void CloseMainWindow()
    {
        if (mainWindow != null)
        {
            Application.Current.MainWindow = mainWindow;
            mainWindow.Close();
        }

    }

    public void ShowMainWindow()
    {
        if (mainWindow == null)
        {
            mainWindow = _host.Services.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = mainWindow;

            mainWindow = new MainWindow();
        }

        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        mainWindow.Activate();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        SingleInstanceApp.Cleanup();
    }
}

