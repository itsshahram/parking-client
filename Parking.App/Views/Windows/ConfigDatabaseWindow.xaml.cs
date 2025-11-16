namespace Parking.App.Views.Windows;

using static Parking.App.Helpers.Constants;

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
            var testResult = await DatabaseConnectionTester.TestConnectionAsync(dbHostAddress.Text, dbUsername.Text, dbPassword.Password);
            if (testResult.Item1)
            {
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
    private void ShowMessage(bool result, string message)
    {
        if (result)
        {
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
        SaveDbConfigBtn.IsEnabled = false;

        try
        {
            if (string.IsNullOrWhiteSpace(dbHostAddress.Text) ||
                string.IsNullOrWhiteSpace(dbName.Text) ||
                string.IsNullOrWhiteSpace(dbUsername.Text) ||
                string.IsNullOrWhiteSpace(dbPassword.Text))
            {
                ShowMessage(false, "لطفاً تمامی فیلدها را وارد کنید.");
                return;
            }

            var (success, errorMessage) = await DatabaseConnectionTester
                .TestConnectionAsync(dbHostAddress.Text, dbUsername.Text, dbPassword.Password);

            if (!success)
            {
                ShowMessage(false, "اتصال به پایگاه داده با خطا مواجه شد، لطفا مقادیر را چک کنید.");
                return;
            }

            var aesKey = AesEncryption.LoadOrCreateAesKey(SecKeyPath);

            string encryptedDbName =
                Convert.ToBase64String(AesEncryption.Encrypt(dbName.Text, aesKey));

            string encryptedDbUser =
                Convert.ToBase64String(AesEncryption.Encrypt(dbUsername.Text, aesKey));

            string encryptedDbHost =
                Convert.ToBase64String(AesEncryption.Encrypt(dbHostAddress.Text, aesKey));

            string encryptedDbPassword =
                Convert.ToBase64String(AesEncryption.Encrypt(dbPassword.Password, aesKey));

            Settings.Default.Application_DbName = encryptedDbName;
            Settings.Default.Application_DbUsername = encryptedDbUser;
            Settings.Default.Application_DbHostAddress = encryptedDbHost;
            Settings.Default.Application_DbPassword = encryptedDbPassword;

            Settings.Default.Application_DbActiveStatus = true;
            Settings.Default.Save();

            ShowMessage(true, "اطلاعات پایگاه داده با موفقیت ذخیره شد.");
        }
        catch (Exception ex)
        {
            ShowMessage(false, "خطا در ذخیره کردن اطلاعات پایگاه داده.");
        }
        finally
        {
            SaveDbConfigBtn.IsEnabled = true;
        }
    }
}
