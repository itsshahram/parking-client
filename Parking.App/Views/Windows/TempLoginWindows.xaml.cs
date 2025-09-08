using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for TempLoginWindows.xaml
    /// </summary>
    public partial class TempLoginWindows : Window
    {
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        public TempLoginWindows()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            Username = txtUsername.Text;
            Password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ShowMessage("", "لطفا ایمیل و پسورد را وارد کنید");
                return;
            }

            DialogResult = true;
            Close();
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
    }
}
