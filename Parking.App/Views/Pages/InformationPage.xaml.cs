using Parking.App.Views.Pages.CardsPageChilds;
using System.Windows.Navigation;

namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for InformationPage.xaml
    /// </summary>
    public partial class InformationPage : Page
    {
        private readonly IParkingService _parkingService;
        public DataPageViewModel ViewModel { get; set; }
        public InformationPage()
        {
            _parkingService = App.GetService<IParkingService>();
            ViewModel = new DataPageViewModel();
            this.DataContext = ViewModel;
            SetData();
            InitializeComponent();
        }

        private void SetData()
        {
            ViewModel.ParkingName = ParkingLotInfoStore.ParkingInfo.Name ?? "_";
            ViewModel.TotalCards = _parkingService.GetCardsCount();
            ViewModel.TotalSpaces = _parkingService.GetSpacesCount() ?? 0;
            ViewModel.OccupiedSpaces = (_parkingService.GetSpacesCount() - _parkingService.GetFreeSpacesCount()).Value;
            ViewModel.TotalDiscountedCards = _parkingService.GetDiscountedCardsCount();
        }

        private void CardWithDisCount_LinkButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new AddCardHistoryPage();

            NavigationService.Navigate(page);
        }
    }
}

