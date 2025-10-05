using Parking.App.Views.Windows.LicensePlate;
using Parking.Domain.Entities.Vehicles;
using Button = System.Windows.Controls.Button;

namespace Parking.App.Views.Pages.LicensePlate;

public partial class LicensePlateGroupPage : Page
{
    private readonly IParkingService _parkingService;
    public LicensePlatePageViewModel ViewModel { get; }

    public LicensePlateGroupPage()
    {
        InitializeComponent();
        _parkingService = App.GetService<IParkingService>();
        ViewModel = new LicensePlatePageViewModel(_parkingService);
        DataContext = ViewModel;
    }

    private async void AddLicensePlate_Click(object sender, RoutedEventArgs e)
    {
        AddLicenseGroupPlateWindows windows = new AddLicenseGroupPlateWindows();
        windows.Owner = Application.Current.MainWindow;

        if (windows.ShowDialog() == true)
        {
            var licensePlateGroup = windows.Vm;
            if (licensePlateGroup.EndDate < licensePlateGroup.StartDate)
            {
                ShowMessage("خطا", $"تاریخ پایان نمی تواند کوچک تر از تاریخ شروع باشد");
                return;
            }
            await _parkingService.AddLicensePlateGroup(new LicensePlateGroup()
            {
                Name = licensePlateGroup.Name,
                StartDate = licensePlateGroup.StartDate,
                Description = licensePlateGroup.Description,
                DiscountPercent = licensePlateGroup.DiscountPercent,
                EndDate = licensePlateGroup.EndDate.Value,
            });
            await ViewModel.LoadData();
        }
    }

    private async void EditLicensePlateGroup_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;

        var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
        if (dataGridRow == null)
            return;

        var item = dataGridRow.Item as LicensePlateListItemResult;
        if (item == null)
            return;

        var group = _parkingService.GetLicensePlateGroupById(item.Id);

        if (group == null)
            return;

        var window = new EditLicenseGroupPlateWindow(group)
        {
            Owner = App.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            var vm = window.Vm;
            var licensePlateGroup = new LicensePlateGroup()
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                DiscountPercent = vm.DiscountPercent,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate.Value,
            };
            await _parkingService.UpdateLicensePlateGroup(licensePlateGroup);

            await ViewModel.LoadData();
        }
    }

    private async void DeleteLicensePlateGroup_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null)
            return;

        var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
        if (dataGridRow == null)
            return;

        var item = dataGridRow.Item as LicensePlateListItemResult;
        if (item == null)
            return;

        var confirmWindow = new ConfirmWindow(
            "حذف گروه پلاک",
            $"آیا از حذف گروه پلاک {item.Name} اطمینان دارید؟",
            ConfirmType.Delete,
            "حذف",
            "انصراف"
        );

        if (confirmWindow.ShowDialog() == true)
        {
            var result = await _parkingService.DeleteLicensePlateGroup(item.Id);
            if (result.IsExsist)
            {
                ShowMessage("خطا", $"امکان حذف گروه پلاک {item.Name} وجود ندارد، زیرا تعدادی پلاک از این گروه استفاده می‌کنند.");
                return;
            }
            ViewModel.LicensePlateGroups.Remove(item);
            ShowMessage("موفقیت", $"گروه پلاک {item.Name} با موفقیت حذف شد.");
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