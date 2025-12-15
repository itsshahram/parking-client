namespace Parking.App.Views.Components;

/// <summary>
/// Interaction logic for TicketDetails.xaml
/// </summary>
public partial class TicketDetails : UserControl
{
    private DispatcherTimer _collapseTimer;
    private DispatcherTimer _progressTimer;
    public TicketDetailsViewModel _customViewModelInstance = new TicketDetailsViewModel();

    public void StartSequence(int seconds)
    {
        StartProgressAnimation(seconds);
        StartCollapseTimer(seconds);
    }
    private void StartCollapseTimer(int seconds)
    {
        if (_collapseTimer != null)
        {
            _collapseTimer.Stop();
        }

        _collapseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(seconds)
        };
        _collapseTimer.Tick += CollapseTimer_Tick;
        _collapseTimer.Start();
    }
    private void CollapseTimer_Tick(object? sender, EventArgs e)
    {
        MainBorder.Visibility = Visibility.Collapsed;
        _collapseTimer.Stop();
    }
    private void StartProgressAnimation(int seconds)
    {
        if (_progressTimer != null)
        {
            _progressTimer.Stop();
        }

        MyProgressBar.Value = 0;
        MyProgressBar.Maximum = 100;
        var unit = MyProgressBar.Maximum / (seconds * 10);

        _progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(seconds * 10)
        };
        _progressTimer.Tick += (sender, e) =>
        {
            if (MyProgressBar.Value < MyProgressBar.Maximum)
            {
                MyProgressBar.Value += unit;
            }
            else
            {
                MyProgressBar.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 100));
                _progressTimer.Stop();
            }
        };
        _progressTimer.Start();
    }
    public static readonly DependencyProperty BackgroundColorProperty =
    DependencyProperty.Register(
        nameof(BackgroundColor),
        typeof(System.Windows.Media.Brush),
        typeof(TicketDetails),
        new PropertyMetadata(new SolidColorBrush(System.Windows.Media.Color.FromArgb(32, 185, 185, 185))));

    public System.Windows.Media.Brush BackgroundColor
    {
        get => (System.Windows.Media.Brush)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }
    public void SetContent(TicketDetailsViewModel content)
    {
        _customViewModelInstance = content;
        DataContext = _customViewModelInstance;
    }
    public TicketDetails()
    {
        //_parkingService = App.GetService<IParkingService>();
        //_customViewModelInstance = new TicketDetailsViewModel();
        DataContext = _customViewModelInstance;
        InitializeComponent();
        BackgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(32, 185, 185, 185));
    }
}
