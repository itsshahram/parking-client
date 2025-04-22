using Parking.App.ViewModels.Pages;
using Parking.App.ViewModels.Windows;
using Parking.App.Views.Pages.SettingsPageChilds;
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
using Wpf.Ui.Controls;

namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get; }
        public SettingsPage()
        {
            ViewModel = new SettingsPageViewModel();
            DataContext = this;
            InitializeComponent();
        }
    }

}
