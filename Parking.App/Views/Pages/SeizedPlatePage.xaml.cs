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
                LoadAddSizedButtonBasedOnRole();
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

        private void LoadAddSizedButtonBasedOnRole()
        {
            if (TokenStore.RoleName == "ParkingManager")
                Add_Seized_Button.Visibility = Visibility.Visible;
            else
                Add_Seized_Button.Visibility = Visibility.Collapsed;
        }

        private void Add_Seized_Click(object sender, RoutedEventArgs e)
        {
            AddSeizedPelakWindow addSeizedPelakWindow = new AddSeizedPelakWindow(() =>
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
    }
}
