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
            var item = _parkingService.GetLicensePlateGroupList();
            ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(item);
        }
    }
}
