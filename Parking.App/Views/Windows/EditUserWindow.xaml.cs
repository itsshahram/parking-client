using Parking.App.Models.Dto.User;

namespace Parking.App.Views.Windows
{
    public partial class EditUserWindow : FluentWindow
    {
        private readonly IUserService _userService;
        public EditUserWindowViewModel ViewModel { get; private set; }

        public EditUserWindow(UserListItemModel user)
        {
            InitializeComponent();
            _userService = App.GetService<IUserService>();
            ViewModel = new EditUserWindowViewModel
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Username = user.UserName
            };
            DataContext = ViewModel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as EditUserWindowViewModel;

            if (vm == null || vm.HasErrors ||
                string.IsNullOrWhiteSpace(vm.FirstName) ||
                string.IsNullOrWhiteSpace(vm.LastName) ||
                string.IsNullOrWhiteSpace(vm.Username))
            {
                ShowMessage("خطا", "لطفاً همه فیلدهای الزامی را پر کنید.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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