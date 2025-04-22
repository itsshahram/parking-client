

namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : FluentWindow
    {
        private readonly Logger<ChangePasswordWindow> logger;
        private readonly IUserService _userService;
        private ChangePasswordWindowViewModel ViewModel { get; set; } = new();
        public ChangePasswordWindow(Guid userId)
        {
            logger = App.GetService<Logger<ChangePasswordWindow>>();
            _userService = App.GetService<IUserService>();
            var UserInfo = _userService.GetUserById(userId);
            ViewModel.UserId = userId;
            ViewModel.UserName = UserInfo?.UserName;
            ViewModel.UserFullName = UserInfo?.Firstname + " " + UserInfo?.Lastname;
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordTextBox.Password != null && ConfirmPasswordTextBox.Password != null)
            {
                if (PasswordTextBox.Password.Length > 1 && ConfirmPasswordTextBox.Password.Length > 1)
                    SavePasswordBtn.IsEnabled = true;
            }
            else
            {
                SavePasswordBtn.IsEnabled = false;
            }
        }

        private void SavePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            //var oldpass = OldPasswordTextBox.Password;
            var newpass = PasswordTextBox.Password;
            var confirmpass = ConfirmPasswordTextBox.Password;
            if (newpass != confirmpass)
            {
                ShowMessage("خطا", "رمز عبور و تکرار آن یکسان نمیباشد");
                return;
            }
            else
            {
                var result = _userService.ChangePassword(ViewModel.UserId, newpass);
                if (result)
                {
                    this.Close();
                }
                else
                {
                    ShowMessage("خطا", "تغییر رمز عبور با خطا مواجه شد.");
                }
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
                        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                        ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                        ms.Title = title;
                        ms.Content = message;
                        ms.IsPrimaryButtonEnabled = false;
                        ms.IsSecondaryButtonEnabled = false;
                        ms.CloseButtonText = "متوجه شدم";
                        await ms.ShowDialogAsync();
                    });

                }
            }
            catch
            {
                System.Windows.MessageBox.Show(message, title);
            }
        }
    }
}
