using Parking.App.Models.Dto.Vehicle.LicensePlate;
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
    }
}
