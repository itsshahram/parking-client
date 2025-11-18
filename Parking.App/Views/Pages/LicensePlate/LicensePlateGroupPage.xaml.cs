using Parking.App.Views.Windows.LicensePlate;
using Parking.Domain.Entities.Vehicles;
using Button = System.Windows.Controls.Button;

namespace Parking.App.Views.Pages.LicensePlate
{
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
            Loaded += LicensePlateGroupPage_Loaded;
        }

        private async void LicensePlateGroupPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= LicensePlateGroupPage_Loaded;
            await ViewModel.LoadDataAsync();
        }

        private async void PaginationControl_PageChanged(object sender, int newPage)
        {
            ViewModel.CurrentPage = newPage;
            await ViewModel.LoadDataAsync();
        }

        private async void AddLicensePlate_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddLicenseGroupPlateWindows
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                var vm = window.Vm;
                if (vm.EndDate < vm.StartDate)
                {
                    await ShowMessage("خطا", "تاریخ پایان نمی‌تواند کوچکتر از تاریخ شروع باشد.");
                    return;
                }

                var result = await _parkingService.AddLicensePlateGroup(new LicensePlateGroup
                {
                    Name = vm.Name,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate!.Value,
                    Description = vm.Description,
                    DiscountPercent = vm.DiscountPercent,
                });

                if (!result)
                {
                    await ShowMessage("خطا", "ثبت گروه با خطا مواجه شد.");
                    return;
                }

                await ShowMessage("موفقیت", "گروه جدید با موفقیت افزوده شد.");
                await ViewModel.LoadDataAsync();
            }
        }

        private async void EditLicensePlateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not LicensePlateGroupCardViewModel item)
                return;

            var group = _parkingService.GetLicensePlateGroupById(item.Id);
            if (group == null)
                return;

            var window = new EditLicenseGroupPlateWindow(group)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                var vm = window.Vm;
                var updatedGroup = new LicensePlateGroup
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Description = vm.Description,
                    DiscountPercent = vm.DiscountPercent,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate!.Value
                };

                await _parkingService.UpdateLicensePlateGroup(updatedGroup);
                await ShowMessage("موفقیت", "گروه به‌روزرسانی شد.");
                await ViewModel.LoadDataAsync();
            }
        }

        private async void DeleteLicensePlateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not LicensePlateGroupCardViewModel item)
                return;

            var confirmWindow = new ConfirmWindow(
                "حذف گروه پلاک",
                $"آیا از حذف گروه پلاک {item.Name} اطمینان دارید؟",
                ConfirmType.Delete,
                "حذف",
                "انصراف");

            if (confirmWindow.ShowDialog() == true)
            {
                var result = await _parkingService.DeleteLicensePlateGroup(item.Id);
                if (result.IsExsist)
                {
                    await ShowMessage("خطا", $"امکان حذف گروه پلاک {item.Name} وجود ندارد، زیرا پلاک فعال دارد.");
                    return;
                }

                ViewModel.LicensePlateGroups.Remove(item);
                await ShowMessage("موفقیت", $"گروه {item.Name} حذف شد.");
                await ViewModel.LoadDataAsync();
            }
        }

        private async void AddPlateToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not LicensePlateGroupCardViewModel group)
                return;

            var addPlateWindow = new AddLicensePlateWindow(group.Id, true)
            {
                Owner = Application.Current.MainWindow
            };

            if (addPlateWindow.ShowDialog() == true)
            {
                var newPlate = new Domain.Entities.Vehicles.LicensePlate
                {
                    EnLicensePlate = addPlateWindow.EnLicensePlate,
                    IsLocal = true,
                    GroupId = addPlateWindow.SelectedGroupId ?? group.Id
                };

                var result = await _parkingService.AddLicensePlate(newPlate);
                if (result.IsExsist)
                {
                    await ShowMessage("خطا", "امکان تعریف پلاک برای چندین گروه وجود ندارد.");
                    return;
                }

                if (!result.IsSuccess)
                {
                    await ShowMessage("خطا", "ثبت پلاک با خطا مواجه شد.");
                    return;
                }

                await ShowMessage("موفقیت", "پلاک با موفقیت افزوده شد.");
                await ViewModel.LoadDataAsync();
            }
        }

        private async void DeletePlate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.DataContext is not LicensePlatePlateItemViewModel plate ||
                button.Tag is not LicensePlateGroupCardViewModel group)
            {
                return;
            }

            var confirmWindow = new ConfirmWindow(
                "حذف پلاک",
                $"آیا از حذف پلاک {plate.PersianPlate} اطمینان دارید؟",
                ConfirmType.Delete,
                "حذف",
                "انصراف");

            if (confirmWindow.ShowDialog() == true)
            {
                var success = await _parkingService.DeleteLicensePlate(plate.Id);
                if (!success)
                {
                    await ShowMessage("خطا", "حذف پلاک با خطا مواجه شد.");
                    return;
                }

                group.Plates.Remove(plate);
                await ShowMessage("موفقیت", "پلاک با موفقیت حذف شد.");
            }
        }

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentPage = 1;
            await ViewModel.LoadDataAsync();
        }

        private async void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearFilters();
            ViewModel.CurrentPage = 1;
            await ViewModel.LoadDataAsync();
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private async Task ShowMessage(string title, string message)
        {
            try
            {
                if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        var ms = new Wpf.Ui.Controls.MessageBox
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
