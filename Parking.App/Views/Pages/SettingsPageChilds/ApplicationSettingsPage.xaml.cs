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
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private readonly IParkingService _parkingService;
        public ApplicationSettingsPage()
        {
            _parkingService = App.GetService<IParkingService>();

            InitializeComponent();
            var vehicleSegmentsList = _parkingService.GetVehicleSegments().Select(v => new ComboBoxItem { Tag = v.Id, Content = v.NameFa }).ToList();
            foreach (var item in vehicleSegmentsList.OrderBy(v=>v.Tag))
                VehicleSegmentComboBox.Items.Add(item);
            if (Settings.Default.Application_DefaultVehicleSegmentPrice >0)
            {
                VehicleSegmentComboBox.SelectedIndex = vehicleSegmentsList.IndexOf(vehicleSegmentsList.FirstOrDefault(v => (int)v.Tag == Settings.Default.Application_DefaultVehicleSegmentPrice));
            }
            
        }
        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            Settings.Default.Application_GateType = selectedItem?.Tag?.ToString() ?? "0";
            Settings.Default.Save();
        }

        private void VehicleSegmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Settings.Default.Application_DefaultVehicleSegmentPrice = short.Parse(selectedItem.Tag.ToString());
                Settings.Default.Save();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Application_MissingCardPrice = decimal.Parse(((TextBox)sender).Text);
            Settings.Default.Save();
        }
        private void GateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Application_GatePCName = ((TextBox)sender).Text;
            Settings.Default.Save(); 
        }
    }
}
