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

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for AccountsSettingsPage.xaml
    /// </summary>
    public partial class SyncConfigPage : Page
    {
        private readonly ILogger<SyncConfigPage> _logger;
        private readonly ISynchronizationService _synchronizationService;
        public SyncConfigPage()
        {
            _logger = App.GetService<ILogger<SyncConfigPage>>();
            _synchronizationService = App.GetService<ISynchronizationService>();
            InitializeComponent();
        }

        private async void SyncUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _synchronizationService.GetParkingLotAccountsFromServerAsync();
            }
            catch (Exception ex) {
                 _logger.LogError(ex, "SyncUsers_Click");
            }
        }

        private async void SyncPriceList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _synchronizationService.ReceiveVehicleSegmentsListFromServerAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncUsers_Click");
            }
        }
    }
}
