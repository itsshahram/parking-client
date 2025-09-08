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
