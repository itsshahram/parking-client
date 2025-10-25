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
        }

        #region Pagination

        private async void PaginationControl_PageChanged(object sender, int newPage)
        {
            ViewModel.CurrentPage = newPage;
            await ViewModel.LoadDataAsync();
        }

        private void PaginationControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: trigger first load here if needed
        }

        #endregion

        #region CRUD

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
                    await ShowMessage("خطا", "تاریخ پایان نمی‌تواند کوچک‌تر از تاریخ شروع باشد");
                    return;
                }

                await _parkingService.AddLicensePlateGroup(new LicensePlateGroup
                {
                    Name = vm.Name,
                    StartDate = vm.StartDate,
                    Description = vm.Description,
                    DiscountPercent = vm.DiscountPercent,
                    EndDate = vm.EndDate.Value
                });

                await ViewModel.LoadDataAsync();
            }
        }

        private async void EditLicensePlateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
            if (dataGridRow?.Item is not LicensePlateListItemResult item) return;

            var group = _parkingService.GetLicensePlateGroupById(item.Id);
            if (group == null) return;

            var window = new EditLicenseGroupPlateWindow(group)
            {
                Owner = App.Current.MainWindow
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
                    EndDate = vm.EndDate.Value
                };

                await _parkingService.UpdateLicensePlateGroup(updatedGroup);
                await ViewModel.LoadDataAsync();
            }
        }

        private async void DeleteLicensePlateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
            if (dataGridRow?.Item is not LicensePlateListItemResult item) return;

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
                    await ShowMessage("خطا", $"امکان حذف گروه پلاک {item.Name} وجود ندارد، زیرا تعدادی پلاک از این گروه استفاده می‌کنند.");
                    return;
                }

                ViewModel.LicensePlateGroups.Remove(item);
                await ShowMessage("موفقیت", $"گروه پلاک {item.Name} با موفقیت حذف شد.");
                await ViewModel.LoadDataAsync();
            }
        }

        #endregion

        #region Filters

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentPage = 1;
            await ViewModel.LoadDataAsync();
        }

        private async void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterName = string.Empty;
            ViewModel.FilterDiscount = null;
            ViewModel.FilterStartDate = null;
            ViewModel.FilterEndDate = null;
            ViewModel.CurrentPage = 1;

            startYearTextBox.Text = string.Empty;
            startMonthTextBox.Text = string.Empty;
            startDayTextBox.Text = string.Empty;
            startHourTextBox.Text = string.Empty;
            startMinuteTextBox.Text = string.Empty;

            endYearTextBox.Text = string.Empty;
            endMonthTextBox.Text = string.Empty;
            endDayTextBox.Text = string.Empty;
            endHourTextBox.Text = string.Empty;
            endMinuteTextBox.Text = string.Empty;

            await ViewModel.LoadDataAsync();
        }


        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        #endregion

        #region MessageBox

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

        #endregion

    }
}
