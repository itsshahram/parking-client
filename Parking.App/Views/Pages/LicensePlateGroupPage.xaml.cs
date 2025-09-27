namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for LicensePlateGroupPage.xaml
    /// </summary>
    public partial class LicensePlateGroupPage : Page
    {
        private LicensePlateGroupViewModel ViewModel { get; set; }
        private readonly IParkingService _parkingService;
        private readonly ILogger<LicensePlateGroupPage> _logger;
        public LicensePlateGroupPage()
        {
            ViewModel = new LicensePlateGroupViewModel();
            DataContext = ViewModel;
            _parkingService = App.GetService<IParkingService>();
            _logger = App.GetService<ILogger<LicensePlateGroupPage>>();
            InitializeComponent();
            LoadData();
            try
            {
                var plateChars = LicensePlateHelper.GetChars();
                plateCharsCombo.ItemsSource = plateChars.Select(p => p.PlateFa).ToList();
            }
            catch {
            }
        }

        public void LoadData()
        {
            var (data, totalCount) = _parkingService.GetLicensePlateGroupList(EnLicensePlate, 1, 10);

            ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(data);
            PlateDataGrid.ItemsSource = ViewModel.Items;
            ViewModel.CurrentPage = 1;
            ViewModel.ItemsPerPage = 10;
            ViewModel.TotalCount = totalCount;
        }
        private string? EnLicensePlate { get; set; } = null;
        private void Pagination_PageChanged(object sender, int newPage)
        {
            var (data, totalCount) = _parkingService.GetLicensePlateGroupList(EnLicensePlate, newPage, 10);

            ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(data);
            ViewModel.CurrentPage = newPage;
            ViewModel.ItemsPerPage = 10;
            ViewModel.TotalCount = totalCount;
            resultCount.Text = totalCount.ToString();
            PlateDataGrid.ItemsSource = ViewModel.Items;
        }
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var plateChar = (plateCharsCombo.SelectedItem as string);
            EnLicensePlate = $"{leftNumbersNumberTextBox.Text}_{plateChar?.ConvertFaCharToEnCharIndex()}_{rightNumbersNumberTextBox.Text}" + $"_IR{irNumberTextBox.Text}";
            LoadData();
        }

        private async void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            leftNumbersNumberTextBox.Text = string.Empty;
            rightNumbersNumberTextBox.Text = string.Empty;
            irNumberTextBox.Text = string.Empty;
            EnLicensePlate = null;
            LoadData();
        }
    }
}
