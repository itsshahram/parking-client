

using Wpf.Ui.Controls;

namespace Parking.App.Views.Components;


/// <summary>
/// Interaction logic for ParkingInfo.xaml
/// </summary>
public partial class ParkingInfo : UserControl
{
    private readonly IParkingService _parkingService;
    private readonly ILogger<ParkingInfo> logger;
    private readonly ISynchronizationService _synchronizationService;
    private DispatcherTimer _timer;
    public ParkingInfo()
    {
        _parkingService = App.GetService<IParkingService>();
        _synchronizationService = App.GetService<ISynchronizationService>();
        logger = App.GetService<ILogger<ParkingInfo>>();
        ParkingLotInfoStore.ParkingInfo = _parkingService.GetParkingLotDetails().Result;
        
        InitializeComponent();
        PakingNameText.Text = ParkingLotInfoStore.ParkingInfo?.Name;
        DateText.Text = DateTime.Now.ToLongShamsiString();
        TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        UserFullName.Text = TokenStore.FullName ?? "-----";
        InitializeTimer();
        InitializeServerStatusCheck();
        if (TokenStore.ServerStatus)
        {
            UpdateIconAndColor(SymbolRegular.CloudCheckmark16, "SystemFillColorSuccessBrush", "ارتباط با سرور برقرار میباشد");
        }
        else
        {
            UpdateIconAndColor(SymbolRegular.CloudDismiss16, "SystemFillColorCriticalBrush", "ارتباط با سرور برقرار نمیباشد");
        }
    }
    private void InitializeTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }
    private async void Timer_Tick(object sender, EventArgs e)
    {

        try
        {
            DateText.Text = DateTime.Now.ToLongShamsiString();
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }   catch(Exception ex)
        {
            logger.LogError(ex.Message, ex);
        }

    }

    private DispatcherTimer serverStatusTimer;

    public void InitializeServerStatusCheck()
    {
        serverStatusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) 
        };
        serverStatusTimer.Tick += CheckServerStatus;
        serverStatusTimer.Start();
    }
    private async void CheckServerStatus(object sender, EventArgs e)
    {
        if (Settings.Default.Application_Sync_Enable)
        {
            if (TokenStore.ServerStatus)
            {
                UpdateIconAndColor(SymbolRegular.CloudCheckmark16, "SystemFillColorSuccessBrush", "ارتباط با سرور برقرار میباشد");
            }
            else
            {
                UpdateIconAndColor(SymbolRegular.CloudDismiss16, "SystemFillColorCriticalBrush", "ارتباط با سرور برقرار نمیباشد");
            }
        }

    }



    private void UpdateIconAndColor(SymbolRegular icon, string colorResource, string message)
    {
        // تغییر آیکون و رنگ
        ServerStatusIcon.Symbol = icon;
        ServerStatusIcon.Foreground = (System.Windows.Media.Brush)Application.Current.Resources[colorResource];
        ServerStatusIcon.ToolTip = message;
    }


}
