using Coravel;
using Parking.App.Seeds;
using Parking.App.Views.Pages.LicensePlate;
using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities.User;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Net;
using static Parking.App.Helpers.Constants;
using Log = Serilog.Log;
using MessageBox = System.Windows.MessageBox;

namespace Parking.App
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += UnHandleException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        // === GLOBALS ===
        public IConfiguration? Configuration { get; private set; }
        public Serilog.ILogger _logger;
        public static CancellationTokenSource GlobalCancellationTokenSource { get; private set; } = new();
        public static DatabaseMonitorService DatabaseMonitor { get; private set; }

        private static void LoadDatabaseCredentials()
        {
            var key = AesEncryption.LoadOrCreateAesKey(SecKeyPath);

            string hostBase64 = EnsureEncrypted(
                Settings.Default.Application_DbHostAddress,
                nameof(Settings.Default.Application_DbHostAddress),
                key);

            string dbNameBase64 = EnsureEncrypted(
                Settings.Default.Application_DbName,
                nameof(Settings.Default.Application_DbName),
                key);

            string userBase64 = EnsureEncrypted(
                Settings.Default.Application_DbUsername,
                nameof(Settings.Default.Application_DbUsername),
                key);

            string passBase64 = EnsureEncrypted(
                Settings.Default.Application_DbPassword,
                nameof(Settings.Default.Application_DbPassword),
                key);

            DatabaseCredentials.Host =
                AesEncryption.Decrypt(Convert.FromBase64String(hostBase64), key);

            DatabaseCredentials.DbName =
                AesEncryption.Decrypt(Convert.FromBase64String(dbNameBase64), key);

            DatabaseCredentials.DbUserName =
                AesEncryption.Decrypt(Convert.FromBase64String(userBase64), key);

            DatabaseCredentials.DbPassword =
                AesEncryption.Decrypt(Convert.FromBase64String(passBase64), key);
        }


        private static string BuildConnectionString()
        {
            return $"Server={DatabaseCredentials.Host};" +
                   $"Database={DatabaseCredentials.DbName};" +
                   $"User Id={DatabaseCredentials.DbUserName};" +
                   $"Password={DatabaseCredentials.DbPassword};" +
                   $"TrustServerCertificate=true;MultipleActiveResultSets=True;";
        }


        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                if (Settings.Default.Application_DbActiveStatus)
                {
                    var connectionString = BuildConnectionString();

                    services.AddDbContextFactory<ApplicationDbContext>(opt =>
                        opt.UseSqlServer(connectionString));

                    services.AddIdentity<ApplicationUser, ApplicationRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    services.AddScoped<IParkingService, ParkingService>();
                    services.AddScoped<IRoleService, RoleService>();
                    services.AddScoped<ISynchronizationService, SynchronizationService>();
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<ITicketQueueService, TicketQueueService>();
                    services.AddScoped<IThemeService, ThemeService>();

                    services.AddScheduler();
                    services.AddTransient<BackgroundTask>();

                    // UI Windows & Pages
                    services.AddScoped<MainWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<DatabaseErrorWindow>();
                    services.AddTransient<AddUserWindow>();
                    services.AddTransient<EditUserWindow>();
                    services.AddTransient<TicketDetailsWindow>();
                    services.AddTransient<CustomAmountPaymentModalWindow>();

                    services.AddScoped<MainPage>();
                    services.AddScoped<MainWindowViewModel>();
                    services.AddScoped<SettingsPageViewModel>();
                    services.AddScoped<LicensePlateGroupPage>();
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



        public static T? GetService<T>() where T : class =>
            _host.Services.GetService(typeof(T)) as T;

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
                SingleInstanceApp.ActivatePreviousInstance(); // Restore hidden window
                Shutdown(); // Close the new instance
                return;
            }


            LoadDatabaseCredentials();

            var connectionString = BuildConnectionString();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString);

            try
            {
                bool canConnect = await CanConnectToDatabaseAsync();

                if (!canConnect)
                {
                    ShowDatabaseErrorWindow();
                    return;
                }

                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                    await context.Database.MigrateAsync();

                DatabaseMonitor = new DatabaseMonitorService();
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

        private static string EnsureEncrypted(string value, string keyName, byte[] aesKey)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            if (IsBase64String(value))
                return value;

            var encrypted = Convert.ToBase64String(AesEncryption.Encrypt(value, aesKey));

            switch (keyName)
            {
                case nameof(Settings.Default.Application_DbHostAddress):
                    Settings.Default.Application_DbHostAddress = encrypted;
                    break;
                case nameof(Settings.Default.Application_DbName):
                    Settings.Default.Application_DbName = encrypted;
                    break;
                case nameof(Settings.Default.Application_DbUsername):
                    Settings.Default.Application_DbUsername = encrypted;
                    break;
                case nameof(Settings.Default.Application_DbPassword):
                    Settings.Default.Application_DbPassword = encrypted;
                    break;
            }

            Settings.Default.Save();
            return encrypted;
        }

        private static async Task<bool> CanConnectToDatabaseAsync()
        {
            try
            {
                string host = DatabaseCredentials.Host;
                string user = DatabaseCredentials.DbUserName;
                string pass = DatabaseCredentials.DbPassword;

                var connected = await DatabaseConnectionTester.TestConnectionAsync(host, user, pass);
                return connected.Item1;
            }
            catch
            {
                return false;
            }
        }

        public void ShowMainWindow()
        {
            if (mainWindow == null)
            {
                mainWindow = _host.Services.GetRequiredService<MainWindow>();
                Application.Current.MainWindow = mainWindow;

                mainWindow.Closing += (s, e) =>
                {
                    e.Cancel = true;
                    mainWindow.Hide();
                };
            }

            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;

            if (!mainWindow.IsVisible)
                mainWindow.Show();


            mainWindow.Activate();
            mainWindow.Topmost = true;  
            mainWindow.Topmost = false;
        }


        public void CloseMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.Closing -= null;

                mainWindow.Close();
                mainWindow = null;
                Application.Current.MainWindow = null;
            }
        }


        private static bool _dbErrorWindowOpen = false;

        private static void ShowDatabaseErrorWindow()
        {
            if (_dbErrorWindowOpen) return;
            _dbErrorWindowOpen = true;

            try
            {
                var dbWindow = new DatabaseErrorWindow(DatabaseCredentials.Host)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                if (Application.Current.MainWindow != null &&
                    Application.Current.MainWindow is not LoginWindow &&
                    Application.Current.MainWindow != dbWindow)
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

        private void ConfigureLogging()
        {
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            var logFilePath = Path.Combine(logDirectory, "log.txt");
            var crashFilePath = Path.Combine(logDirectory, "crash-log.txt");

            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("IP_Address", GetLocalIPAddress())
                .MinimumLevel.Information()
                .WriteTo.File(logFilePath,
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    retainedFileCountLimit: null,
                    rollOnFileSizeLimit: false,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(crashFilePath,
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    retainedFileCountLimit: null,
                    rollOnFileSizeLimit: false,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

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
                                Settings.Default.Application_Logs_Elastic_Pass)
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
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }

            return "Unknown";
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

            Environment.Exit(1);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.Error(e.Exception, "Unobserved Task exception occurred.");
            e.SetObserved();
        }

        private static bool IsBase64String(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();

            if (input.Length % 4 != 0) return false;

            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
