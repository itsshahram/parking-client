using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using Button = Wpf.Ui.Controls.Button;


namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for UsersListPage.xaml
    /// </summary>
    public partial class UsersListPage : Page
    {
        private UsersListPageViewModel ViewModel { get; set; } = new();
        private readonly Logger<UsersListPage> logger;
        private readonly IUserService _userService;
        public UsersListPage()
        {
            DataContext = ViewModel;
            _userService = App.GetService<IUserService>();
            var item = _userService.GetAllUsersAsync().Result;
            
            ViewModel.Items = new ObservableCollection<UserListItemModel>(item);
            InitializeComponent();
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                ShowMessage("خطا", "دکمه معتبر نیست!");
                return;
            }
            var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
            if (dataGridRow == null)
            {
                ShowMessage("خطا", "ردیف داده معتبر نیست!");
                return;
            }
            var user = dataGridRow?.Item as UserListItemModel;

            var ChangePassword = new ChangePasswordWindow(user.Id);
            ChangePassword?.Show();
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
