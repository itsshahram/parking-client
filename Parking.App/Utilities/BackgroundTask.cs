using Coravel.Invocable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //_synchronizationService?.SendUnSyncedTicketToServer();
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
                await _synchronizationService.ReceiveLicensePlateGroupFromServerAsync();
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
                await _synchronizationService.SyncTicketExtraImagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Sync Ticket Extra Images", ex.Message);
                SetAppIcon(TaskStatus.Error);
            }
            SetAppIcon(TaskStatus.Success);
            TokenStore.ServerStatus = true;
        }
        catch (Exception ex) {
            _logger.LogError("Error in establishing connection with the server", ex.Message);
            SetAppIcon(TaskStatus.Error);
        }

    }
    public Task Invoke()
    {
        
        try
        {
            if (Settings.Default.Application_Sync_Enable)
            {
                if (_synchronizationService.ServerConnectiviyCheckAsync().Result)
                {

                    if (Settings.Default.Application_Sync_Enable)
                    {
                        SyncData().Wait();
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return Task.CompletedTask;
                    }

                }
                else
                {
                    _logger.LogError("Error in establishing connection with the server");
                    SetAppIcon(TaskStatus.Error);
                    TokenStore.ServerStatus = false;
                    return Task.CompletedTask;
                }
            }
            else
            {
                SetAppIcon(TaskStatus.Error);
                TokenStore.ServerStatus = false;
                return Task.CompletedTask;
            }
                
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in establishing connection with the server",ex.Message);
            SetAppIcon(TaskStatus.Error);
            return Task.CompletedTask;
        }
    }

    private void SetAppIcon(TaskStatus ts)
    {
        Application.Current.Dispatcher.Invoke(() =>
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
    private enum TaskStatus
    {
        Syncing,
        Success,
        Error, 
        Reset
    }
}
