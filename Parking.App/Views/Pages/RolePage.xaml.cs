using Parking.App.Attributes;
using Parking.Domain.Entities.User;

namespace Parking.App.Views.Pages;


[RequiresPermission("Roles", "مشاهده نقش ها")]
public partial class RolePage : Page
{
    private readonly RolePageViewModel ViewModel = new RolePageViewModel();
    private readonly IRoleService _roleService;

    public RolePage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        _roleService = App.GetService<IRoleService>();

        _ = LoadRolesAsync();
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            var roles = await _roleService.GetRoles();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.Items.Clear();
                foreach (var role in roles)
                {
                    ViewModel.Items.Add(role);
                }
            });
        }
        catch (Exception ex)
        {
            ShowMessage("خطا", $"خطا در بارگذاری کاربران: {ex.Message}");
        }
    }

    [RequiresPermission("RoleAdd", "ایجاد نقش")]
    private async void AddRole_Click(object sender, RoutedEventArgs e)
    {
        AddRoleWindow addRoleWindow = new AddRoleWindow();

        addRoleWindow.Owner = Application.Current.MainWindow;

        if (addRoleWindow.ShowDialog() == true)
        {
            var newRole = addRoleWindow.ViewModel;


            var result = await _roleService.Create(new ApplicationRole()
            {
                Name = newRole.Name,
                FaName = newRole.FaName
            });

            if (result.IsExist)
            {
                ShowMessage("خطا", $"نقش {newRole.Name} از قبل ثبت شده است");
                return;
            }
            await LoadRolesAsync();

            ShowMessage("موفقیت", $"نقش  {newRole.Name} با موفقیت ایجاد شد.");
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
