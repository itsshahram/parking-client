using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.Logging;
using Parking.App.Utilities;
using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Net;
using System.Windows.Forms;
using Log = Serilog.Log;

namespace Parking.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IConfiguration? Configuration { get; private set; }
    public static CancellationTokenSource GlobalCancellationTokenSource { get; private set; } = new CancellationTokenSource();


    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        //.ConfigureAppConfiguration((context, config) =>
        //{
        //    config.SetBasePath(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
        //    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        //})
        .ConfigureServices((context, services) =>
        {
            if (Settings.Default.Application_DbActiveStatus)
            {
                // var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                var connectionString = $"Server={Settings.Default.Application_DbHostAddress};Database={Settings.Default.Application_DbName};User Id={Settings.Default.Application_DbUsername};Password={Settings.Default.Application_DbPassword};TrustServerCertificate=true;MultipleActiveResultSets=True;";
                services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);
                services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

                services.AddHttpClient();

                services.AddTransient<IUnitOfWork, UnitOfWork>();
                //services.AddScoped<Func<IUnitOfWork>>(provider => () => provider.GetRequiredService<IUnitOfWork>());
                //services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();


                services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

                services.AddTransient<IParkingService, ParkingService>();
                services.AddTransient<ISynchronizationService, SynchronizationService>();
                services.AddTransient<IUserService, UserService>();

                services.AddScoped<IThemeService, ThemeService>();

                services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

                services.AddScheduler();
                services.AddTransient<BackgroundTask>();

                services.AddScoped<MainWindow>();
                services.AddScoped<LoginWindow>();

                services.AddScoped<MainPage>();
                services.AddScoped<MainWindowViewModel>();
                services.AddTransient<TicketDetailsWindow>();
                services.AddScoped<SettingsPageViewModel>();

                services.AddScoped<LicensePlateGroupPage>();
                services.AddScoped<LicensePlateGroupViewModel>();

                services.AddScoped<AddCardPage>();
                services.AddScoped<AddCardPageViewModel>();

                services.AddScoped<UserManager<ApplicationUser>>();
                // services.AddScoped<RoleManager<ApplicationRole>>();
                services.AddScoped<RoleManager<IdentityRole<Guid>>>();



                services.AddScoped<ChangePasswordWindow>();


                services.AddScoped<UsersListPage>();
                services.AddScoped<UsersListPageViewModel>();
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

    private void OnStartup(object sender, StartupEventArgs e)
    {
        if (Settings.Default.Application_Logging)
        {
            if (Settings.Default.Application_Logging_In_Elastic)
            {
                string password = Settings.Default.Application_Logs_Elastic_Pass;
                Serilog.Log.Logger = new LoggerConfiguration()
                       .Enrich.FromLogContext()
                       .Enrich.WithMachineName()
                       .Enrich.WithProperty("IP_Address", GetLocalIPAddress())
                       .Enrich.WithProperty("MachineName", Settings.Default.Application_GatePCName)
                       .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri($"{Settings.Default.Application_Logs_Elastic_Server}"))
                       {
                           AutoRegisterTemplate = true,
                           IndexFormat = "logs-{0:yyyy.MM.dd}",
                           MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information,
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
                Serilog.Log.Logger = new LoggerConfiguration()
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
        else
        {
            if (Settings.Default.Application_Logging_In_Elastic)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Logger(lc => lc
                      .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Settings.Default.Application_Logs_Elastic_Server))
                      {
                          AutoRegisterTemplate = true,
                          IndexFormat = "parking_",
                          ModifyConnectionSettings = x =>
                              x.BasicAuthentication(Settings.Default.Application_Logs_Elastic_Username, Settings.Default.Application_Logs_Elastic_Pass)
                      }))
                    .CreateLogger();
            }

        }


        Log.Information("Info: Application Started.");
        Log.Error("Error: Application Started.");
        Log.Warning("Warning: Application Started.");


        if (Settings.Default.Application_DbActiveStatus)
        {
            if (!SingleInstanceApp.IsFirstInstance())
            {
                SingleInstanceApp.ActivatePreviousInstance();
                Shutdown();
                return;
            }

            //var builder = new ConfigurationBuilder()
            //   .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location))
            //   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //Configuration = builder.Build();
            //var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var connectionString = $"Server={Settings.Default.Application_DbHostAddress};Database={Settings.Default.Application_DbName};User Id={Settings.Default.Application_DbUsername};Password={Settings.Default.Application_DbPassword};TrustServerCertificate=true;MultipleActiveResultSets=True;";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            try
            {
                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                {
                    context.Database.Migrate();
                }
                //_host.Start();
                //const string mutexName = "Global\\Parking.App";

                // ایجاد Mutex
                //mutex = new Mutex(true, mutexName, out bool isNewInstance);


                //mainWindow = _host.Services.GetRequiredService<MainWindow>();
                //Application.Current.MainWindow = mainWindow;
                //SingleInstanceApp.SetMainWindow(mainWindow);

                //mainWindow.Show();
                //_host.Services.UseScheduler(s => s.Schedule<BackgroundTask>().EverySeconds(Settings.Default.Application_Sync_Interval_CountOfTake));


                _host.Start();
                var login = _host.Services.GetRequiredService<LoginWindow>();

                login.Show();
                //    if (!isNewInstance)
                //{
                //    // اگر برنامه از قبل اجرا شده باشد
                //    MessageBox.Show("برنامه در حال حاضر در حال اجراست.");
                //    Environment.Exit(0); // خروج از برنامه
                //}

                //base.OnStartup(e);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }
        else
        {
            var dbWindow = _host.Services.GetRequiredService<ConfigDatabaseWindow>();
            dbWindow.Show();
        }
    }
    private void MainWindow_Closed(object sender, ExitEventArgs e)
    {
        if (mainWindow != null)
        {
            //App.GlobalCancellationTokenSource.Cancel();
            //App.GlobalCancellationTokenSource.Dispose();

            Application.Current.MainWindow = mainWindow;
            mainWindow.Close();
        }

    }
    public void CloseMainWindow()
    {
        if (mainWindow != null)
        {
            //App.GlobalCancellationTokenSource.Cancel();
            //App.GlobalCancellationTokenSource.Dispose();

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

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        SingleInstanceApp.Cleanup();
    }
}

