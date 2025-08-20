namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for LicensePlateGroupPage.xaml
    /// </summary>
    public partial class LicensePlateGroupPage : Page
    {
        private LicensePlateGroupViewModel ViewModel { get; set; }
        private readonly Logger<LicensePlateGroupPage> logger;
        private readonly IParkingService _parkingService;
        public LicensePlateGroupPage()
        {
            ViewModel = new LicensePlateGroupViewModel();
            DataContext = ViewModel;
            _parkingService = App.GetService<IParkingService>();
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            var (data, totalCount) = _parkingService.GetLicensePlateGroupList(1, 10);

            ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(data);
            PlateDataGrid.ItemsSource = ViewModel.Items;
            ViewModel.CurrentPage = 1;
            ViewModel.ItemsPerPage = 10;
            ViewModel.TotalCount = totalCount;
        }

        private void Pagination_PageChanged(object sender, int newPage)
        {
            var (data, totalCount) = _parkingService.GetLicensePlateGroupList(newPage, 10);

            ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(data);
            ViewModel.CurrentPage = newPage;
            ViewModel.ItemsPerPage = 10;
            ViewModel.TotalCount = totalCount;
            resultCount.Text = totalCount.ToString();
            PlateDataGrid.ItemsSource = ViewModel.Items;
        }
    }
}
