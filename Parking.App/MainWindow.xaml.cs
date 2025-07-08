using Coravel.Scheduling.Schedule.Interfaces;
using Wpf.Ui.Appearance;

namespace Parking.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindowViewModel ViewModel { get; }
   

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        DataContext = this;
        InitializeComponent();
        SystemThemeWatcher.Watch(this);
        Loaded += (_, _) => RootNavigation.Navigate(typeof(MainPage));
        this.Loaded += MainWindow_Loaded;


        scheduler = App.GetService<IScheduler>();
        StartBackgroundTask();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        //await Task.Run(() =>
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        //Loaded += (_, _) => RootNavigation.Navigate(typeof(MainPage));
        //        RootNavigation.Navigate(typeof(MainPage));
        //    });
        //});
    }
    private readonly IScheduler scheduler;
    public BackgroundTask BackgroundTask { get; private set; }
    private void StartBackgroundTask()
    {
        BackgroundTask = new BackgroundTask(this);

        
        scheduler.Schedule(() => BackgroundTask.Invoke())
                 .EverySeconds(Settings.Default.Application_Sync_Interval_Second);
    }
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }

}