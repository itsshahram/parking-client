using Parking.App.Attributes;

namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for SeizedPlatePage.xaml
    /// </summary>
    public partial class SeizedPlatePage : Page
    {
        private readonly IParkingService _parkingService;
        public ObservableCollection<SeizedLicensePlateModel> SeizedPlateList { get; set; }
        public SeizedPlatePage()
        {
            _parkingService = App.GetService<IParkingService>();

            InitializeComponent();
            try
            {
                SeizedPlateList = new ObservableCollection<SeizedLicensePlateModel>(_parkingService.GetSeizedLicensePlatesList());
                DataContext = this;
            }
            catch
            {
                Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                ms.Title = "خطا";
                ms.Content = "خطا در دریافت اطلاعات";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }

        }


        [RequiresPermission("AddSeized", "افزودن پلاک توقیفی")]
        private void Add_Seized_Click(object sender, RoutedEventArgs e)
        {
            AddSeizedPelakWindow addSeizedPelakWindow = new AddSeizedPelakWindow(async () =>
            {
                var updatedList = _parkingService.GetSeizedLicensePlatesList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SeizedPlateList.Clear();
                    foreach (var plate in updatedList)
                        SeizedPlateList.Add(plate);
                });
            });
            addSeizedPelakWindow.Owner = Application.Current.MainWindow;
            addSeizedPelakWindow.ShowDialog();
        }
        private async void OnDeleteRequested(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SeizedLicensePlateModel model)
            {
                var confirmWindows = new ConfirmWindow("حذف پلاک توقیفی", $"آیا از حذف پلاک {model.FaLicensePlate} مطمئن هستید؟", ConfirmType.Delete);

                if (confirmWindows.ShowDialog() == true)
                {
                    try
                    {
                        await _parkingService.DeleteSeizedVehicleAsync(model.Id);
                        var updatedList = _parkingService.GetSeizedLicensePlatesList();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SeizedPlateList.Clear();
                            foreach (var plate in updatedList)
                                SeizedPlateList.Add(plate);
                        });
                    }
                    catch
                    {
                        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                        ms.Title = "خطا";
                        ms.Content = "خطا در حذف اطلاعات";
                        ms.IsPrimaryButtonEnabled = false;
                        ms.IsSecondaryButtonEnabled = false;
                        ms.CloseButtonText = "متوجه شدم";
                        await ms.ShowDialogAsync();
                        return;
                    }
                }   
            }
        }
    }
}
