using Parking.App.Helpers;
using Parking.App.Services.Interfaces;
using Parking.App.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            ViewModel.TotalSpaces = _parkingService.GetSpacesCount();
            ViewModel.OccupiedSpaces = _parkingService.GetSpacesCount() - _parkingService.GetFreeSpacesCount();
        }
    }
}
