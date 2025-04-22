using Parking.App.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
using Wpf.Ui.Appearance;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for AppearanceSettingsPage.xaml
    /// </summary>
    public partial class AppearanceSettingsPage : Page
    {
        public AppearanceSettingsViewModel ViewModel { get; }
        public AppearanceSettingsPage()
        {
            ViewModel = new AppearanceSettingsViewModel();
            DataContext = this;
            InitializeComponent();


            
            switch (Settings.Default.Appearance_Theme)
            {
                case "theme_light":
                    lightRadio.IsChecked = true;
                    break;
                case "theme_system":
                    darkRadio.IsChecked = true;
                    break;
                default:
                    systemRadio.IsChecked = true;
                    break;
            }

            //InvoiceShowTime.SelectedItem = Settings.Default.Appearance_InvoiceShowTime;
            //InvoiceShowTime.SelectedItem = Settings.Default.Appearance_UpdateListInterval;
        }


        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void MainPageDataUpdateInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            int interval = int.Parse(selectedItem.Tag.ToString());

            Settings.Default.Appearance_UpdateListInterval = interval;
            Settings.Default.Save(); 
        }

        private void InvoiceShowTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            int interval = int.Parse(selectedItem.Tag.ToString());
            Settings.Default.Appearance_InvoiceShowTime = interval;
            Settings.Default.Save();
        }
    }
}
