using Parking.App.Helpers;
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
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConfigDatabaseWindow.xaml
    /// </summary>
    public partial class ConfigDatabaseWindow : FluentWindow
    {
        public ConfigDatabaseWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dbHostAddress.Text != null
              && dbName.Text != null
              && dbUsername.Text != null
              && dbPassword.Text != null)
            {
                TestDbConnectionBtn.IsEnabled = true;
            }
            else
            {
                TestDbConnectionBtn.IsEnabled = false;
            }
        }

        private async void TestDbConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dbHostAddress.Text != null
                  && dbName.Text != null
                  && dbUsername.Text != null
                  && dbPassword.Text != null)
            {
               var testResult = await DatabaseConnectionTester.TestConnectionAsync(dbHostAddress.Text, dbUsername.Text, dbPassword.Text);
                if (testResult.Item1) {
                    ShowMessage(true, "اتصال به پایگاه داده با موفقیت انجام شد، ذخیره کنید");
                    SaveDbConfigBtn.IsEnabled = true;
                }
                else
                {
                    SaveDbConfigBtn.IsEnabled = false;
                    ShowMessage(false, "اتصال به پایگاه داده با خطا مواجه شد، لطفا مقادیر را چک کنید");
                }
            }
        }
        private void ShowMessage(bool result,  string message)
        {
            if (result) {
                msgBox.Visibility = Visibility.Visible;
                connectionTestMessage.Text = message;
                msgBox.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 108, 224, 154));
            }
            else
            {
                msgBox.Visibility = Visibility.Visible;
                connectionTestMessage.Text = message;
                msgBox.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 0, 0));
            }

        }

        private async void SaveDbConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dbHostAddress.Text != null
                  && dbName.Text != null
                  && dbUsername.Text != null
                  && dbPassword.Text != null)
            {
                var testResult = await DatabaseConnectionTester.TestConnectionAsync(dbHostAddress.Text, dbUsername.Text, dbPassword.Text);
                if (testResult.Item1)
                {
                    Settings.Default.Application_DbName = dbName.Text;
                    Settings.Default.Application_DbUsername = dbUsername.Text;
                    Settings.Default.Application_DbHostAddress = dbHostAddress.Text;
                    Settings.Default.Application_DbPassword = dbPassword.Text;
                    Settings.Default.Application_DbActiveStatus = true;
                    Settings.Default.Save();
                    ShowMessage(true, "اطلاعات پایگاه داده با موفقیت ذخیره شد");
                }
                else
                {
                    SaveDbConfigBtn.IsEnabled = false;
                    ShowMessage(false, "اتصال به پایگاه داده با خطا مواجه شد، لطفا مقادیر را چک کنید");
                }
            }
        }
    }
}
