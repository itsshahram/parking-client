namespace Parking.App.Views.Windows
{
    public partial class AddUserWindow : FluentWindow
    {
        private readonly IRoleService _roleService;
        public AddUserWindowViewModel ViewModel { get; private set; }

        public AddUserWindow()
        {
            InitializeComponent();
            _roleService = App.GetService<IRoleService>();
            ViewModel = new AddUserWindowViewModel();
            DataContext = ViewModel;
            _ = LoadRoleAsync();
        }
        private async Task LoadRoleAsync()
        {
            ViewModel.Roles = await _roleService.GetRoles();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddUserWindowViewModel vm)
            {
                vm.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AddUserWindowViewModel;

            if (vm == null || vm.HasErrors ||
                string.IsNullOrWhiteSpace(vm.FirstName) ||
                string.IsNullOrWhiteSpace(vm.LastName) ||
                string.IsNullOrWhiteSpace(vm.Username) ||
                string.IsNullOrWhiteSpace(vm.Password) ||
                string.IsNullOrWhiteSpace(vm.Role))
            {
                ShowMessage("خطا", "لطفاً همه فیلدهای الزامی را پر کنید.");
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class RoleItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}