using Parking.App.Attributes;
using Parking.App.Models.Dto.User;
using Parking.Domain.Entities.User;
using System.ComponentModel;
using Button = Wpf.Ui.Controls.Button;


namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for UsersListPage.xaml
    /// </summary>
    public partial class UsersListPage : Page
    {
        private UsersListPageViewModel ViewModel { get; set; }
        private readonly Logger<UsersListPage> logger;
        private readonly IUserService _userService;
        public UsersListPage()
        {
            InitializeComponent();
            _userService = App.GetService<IUserService>();
            ViewModel = new UsersListPageViewModel(_userService);
            DataContext = ViewModel;

            _ = LoadUsersAsync();
        }

        [RequiresPermission("ChangeUserStatus", "تغییر وضعیت کاربر")]
        private async void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserListItemModel.IsActive))
            {
                var user = sender as UserListItemModel;
                if (user != null)
                {
                    try
                    {
                        if (user.Id != TokenStore.UserId)
                        {
                            await _userService.ChangeStaus(user.Id, user.IsActive);
                            ShowMessage("موفقیت", $"وضعیت کاربر {user.UserName} بروزرسانی شد.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("خطا", $"خطا در بروزرسانی وضعیت کاربر: {ex.Message}");
                    }
                }
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ViewModel.Items.Clear();
                    foreach (var user in users)
                    {
                        user.PropertyChanged += User_PropertyChanged;
                        ViewModel.Items.Add(user);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage("خطا", $"خطا در بارگذاری کاربران: {ex.Message}");
            }
        }

        [RequiresPermission("UserChangePassword", "تغییر رمز عبور")]
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

        [RequiresPermission("UserAdd", "ایجاد کاربر")]
        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = addUserWindow.ShowDialog();
            if (result == true)
            {
                var newUser = addUserWindow.ViewModel;

                var resultUser = await _userService.CreateUser(new ApplicationUser()
                {
                    Firstname = newUser.FirstName,
                    Lastname = newUser.LastName,
                    UserName = newUser.Username,
                    IsActive = true,
                    RegisterDate = DateTime.Now,
                }, newUser.Role, newUser.Password);

                if (resultUser.IsExist)
                {
                    ShowMessage("خطا", $"کاربر {newUser.Username} از قبل ثبت شده است");
                    return;
                }

                await LoadUsersAsync();

                ShowMessage("موفقیت", $"کاربر {newUser.Username} با موفقیت ایجاد شد.");
            }
        }
    }
}
