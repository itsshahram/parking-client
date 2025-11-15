using Coravel.Invocable;

namespace Parking.App.Utilities;

public class BackgroundTask : IInvocable
{
    private readonly ISynchronizationService? _synchronizationService;
    private readonly ILogger<BackgroundTask> _logger;
    private readonly IParkingService? _parkingService;
    private readonly MainWindow _mainWindow;

    public BackgroundTask(MainWindow mainWindow)
    {
        _parkingService = App.GetService<IParkingService>();
        _synchronizationService = App.GetService<ISynchronizationService>();
        _logger = App.GetService<ILogger<BackgroundTask>>();
        _mainWindow = mainWindow;

    }
    public async Task SyncData()
    {
        try
        {
            SetAppIcon(TaskStatus.Syncing);
            try
            {
                await _synchronizationService.SendUnSyncedTicketToServerAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Send UnSynced Tickets To Server", ex.Message);
                SetAppIcon(TaskStatus.Error);
            }
            try
            {
                if (Settings.Default.Application_Sync_EnableSyncLicensePlateGroup)
                {
                    await _synchronizationService.ReceiveLicensePlateGroupFromServerAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Receive LicensePlateGroup From Server", ex.Message);
                SetAppIcon(TaskStatus.Error);
            }
            try
            {
                await _synchronizationService.ReceiveSeizedLicensePlateFromServerAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Receive Seized LicensePlate From Server", ex.Message);
                SetAppIcon(TaskStatus.Error);
            }
            if (Settings.Default.Application_EnableSyncImage)
            {
                try
                {
                    await _synchronizationService.SyncTicketImageAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Sync Ticket Image", ex.Message);
                    SetAppIcon(TaskStatus.Error);
                }
                try
                {
                    _synchronizationService.SyncTicketExitImage();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Sync Ticket Exit Image", ex.Message);
                    SetAppIcon(TaskStatus.Error);
                }
                try
                {
                    await _synchronizationService.SyncTicketExtraImagesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Sync Ticket Extra Images", ex.Message);
                    SetAppIcon(TaskStatus.Error);
                }
            }

            SetAppIcon(TaskStatus.Success);
            TokenStore.ServerStatus = true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in establishing connection with the server", ex.Message);
            SetAppIcon(TaskStatus.Error);
        }
    }
    public async Task Invoke()
    {
        try
        {
            if (Settings.Default.Application_Sync_Enable)
            {
                if (await _synchronizationService.ServerConnectiviyCheckAsync())
                {

                    if (Settings.Default.Application_Sync_Enable)
                    {
                        await SyncData();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _logger.LogError("Error in establishing connection with the server");
                    SetAppIcon(TaskStatus.Error);
                    TokenStore.ServerStatus = false;
                    return;
                }
            }
            else
            {
                SetAppIcon(TaskStatus.Error);
                TokenStore.ServerStatus = false;
                return;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError("Error in establishing connection with the server", ex.Message);
            SetAppIcon(TaskStatus.Error);
            return;
        }
    }

    private void SetAppIcon(TaskStatus ts)
    {
        try
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (ts)
                {
                    case TaskStatus.Syncing:
                        _mainWindow?.myNotifyTray.SetSyncingIcon();
                        break;

                    case TaskStatus.Success:
                        _mainWindow?.myNotifyTray.ResetIcon(); // ResetIcon می‌تواند به وضعیت عادی تغییر کند
                        break;

                    case TaskStatus.Error:
                        _mainWindow?.myNotifyTray.SetErrorIcon("Error in establishing connection with the server");
                        break;

                    case TaskStatus.Reset:
                        _mainWindow?.myNotifyTray.ResetIcon(); // Reset به وضعیت پیش‌فرض برگردانده می‌شود
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(ts), ts, "Unknown TaskStatus");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in Set App Icon", ex.Message);
        }

    }
    private enum TaskStatus
    {
        Syncing,
        Success,
        Error,
        Reset
    }
}
