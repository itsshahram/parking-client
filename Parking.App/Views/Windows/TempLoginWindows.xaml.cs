using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Parking.App.Views.Windows;

public partial class TempLoginWindows : Window
{
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;

    private readonly ISynchronizationService _synchronizationService;

    public TempLoginWindows()
    {
        InitializeComponent();
        _synchronizationService = App.GetService<ISynchronizationService>();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        Username = usernameBox.Text;
        Password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ShowMessage("ورود به حساب", "لطفا نام کاربری و رمز عبور را وارد کنید");
            return;
        }

        btnLogin.IsEnabled = false;
        usernameBox.IsEnabled = false;
        passwordBox.IsEnabled = false;
        LoadingRing.Visibility = Visibility.Visible;

        var previousButtonContent = btnLogin.Content;
        btnLogin.Content = "در حال ورود...";

        try
        {
            var loginResult = await _synchronizationService.CheckTokenAsync(Username, Password);

            if (loginResult?.Succeeded == true)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ShowMessage("ورود به حساب", "نام کاربری یا رمز عبور اشتباه است");
            }
        }
        catch (Exception ex)
        {
            ShowMessage("ورود به حساب", "خطا در ارتباط با سرور. دوباره تلاش کنید.");
        }
        finally
        {
            LoadingRing.Visibility = Visibility.Collapsed;
            btnLogin.IsEnabled = true;
            usernameBox.IsEnabled = true;
            passwordBox.IsEnabled = true;
            btnLogin.Content = previousButtonContent;
        }
    }

    private async void ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    MessageBox ms = new MessageBox
                    {
                        FlowDirection = System.Windows.FlowDirection.RightToLeft,
                        Title = title,
                        Content = message,
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false,
                        CloseButtonText = "متوجه شدم"
                    };
                    await ms.ShowDialogAsync();
                });
            }
        }
        catch
        {
            System.Windows.MessageBox.Show(message, title);
        }
    }

    private void usernameBox_TextChanged(object sender, RoutedEventArgs e) { }
    private void passwordBox_TextChanged(object sender, RoutedEventArgs e) { }
}
