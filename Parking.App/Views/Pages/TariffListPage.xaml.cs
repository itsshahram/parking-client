using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for TariffListPage.xaml
    /// </summary>
    public partial class TariffListPage : Page
    {
        private readonly IParkingService _parkingService;
        public ObservableCollection<VehicleSegmentPriceListItemModel> VehicleSegmentPriceList { get; set; }


        public TariffListPage()
        {
            _parkingService = App.GetService<IParkingService>();

            InitializeComponent();
            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                var x = await _parkingService?.GetVehicleSegmentPriceListAsync();
                VehicleSegmentPriceList = new ObservableCollection<VehicleSegmentPriceListItemModel>(x);
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
                await ms.ShowDialogAsync();
                return;
            }
        }
    }
}
