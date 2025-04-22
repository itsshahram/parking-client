using Coravel.Scheduling.Schedule;
using Coravel.Scheduling.Schedule.Interfaces;
using Parking.App.Utilities;
using Parking.App.ViewModels.Windows;
using Parking.App.Views.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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