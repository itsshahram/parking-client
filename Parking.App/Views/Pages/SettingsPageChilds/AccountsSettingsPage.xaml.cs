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
    public partial class AccountsSettingsPage : Page
    {
        private readonly ILogger<AccountsSettingsPage> _logger;
        public AccountsSettingsPage()
        {
            _logger = App.GetService<ILogger<AccountsSettingsPage>>();
            InitializeComponent();
            UserNameText.Text = TokenStore.Username;
        }

        private async void LogOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = (App)App.Current;
                app.CloseMainWindow();
                var login = App.GetService<LoginWindow>();
                login?.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                ms.Title = "خطا";
                ms.Content = "خطا در خروج از برنامه";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                await ms.ShowDialogAsync();
            }
        }
    }
}
