using Parking.App.Views.Windows.LicensePlate;
using Button = System.Windows.Controls.Button;

namespace Parking.App.Views.Pages.LicensePlate;

/// <summary>
/// Interaction logic for LicensePlateGroupPage.xaml
/// </summary>
public partial class LicensePlatePage : Page
{
    private LicensePlateGroupViewModel ViewModel { get; set; }
    private readonly IParkingService _parkingService;
    private readonly ILogger<LicensePlateGroupPage> _logger;
    private string? EnLicensePlate { get; set; } = null;
    private Guid? SelectedGroupId { get; set; } = null;

    public LicensePlatePage()
    {
        InitializeComponent();
        ViewModel = new LicensePlateGroupViewModel();
        DataContext = ViewModel;

        _parkingService = App.GetService<IParkingService>();
        _logger = App.GetService<ILogger<LicensePlateGroupPage>>();

        LoadGroupFilter();
        LoadData();
        InitPlateChars();
    }

    private void InitPlateChars()
    {
        try
        {
            var plateChars = LicensePlateHelper.GetChars();
            plateCharsCombo.ItemsSource = plateChars.Select(p => p.PlateFa).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing plate characters");
        }
    }

    private async void LoadGroupFilter()
    {
        try
        {
            var data = await _parkingService.GetLicensePlateList();

            GroupFilterComboBox.ItemsSource = data;
            GroupFilterComboBox.DisplayMemberPath = "Name";
            GroupFilterComboBox.SelectedValuePath = "Id";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group filter");
        }
    }

    public void LoadData()
    {
        var (data, totalCount) = _parkingService.GetLicensePlateGroupList(
            EnLicensePlate,
            ViewModel.CurrentPage,
            ViewModel.ItemsPerPage,
            SelectedGroupId
        );

        ViewModel.Items = new ObservableCollection<LicensePlateListItemViewModel>(data);
        ViewModel.TotalCount = totalCount;
        PlateDataGrid.ItemsSource = ViewModel.Items;
    }

    private void Pagination_PageChanged(object sender, int newPage)
    {
        ViewModel.CurrentPage = newPage;
        LoadData();
    }

    private void btnSearch_Click(object sender, RoutedEventArgs e)
    {
        var plateChar = (plateCharsCombo.SelectedItem as string);
        EnLicensePlate = $"{leftNumbersNumberTextBox.Text}_{plateChar?.ConvertFaCharToEnCharIndex()}_{rightNumbersNumberTextBox.Text}_IR{irNumberTextBox.Text}";
        LoadData();
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        leftNumbersNumberTextBox.Text = string.Empty;
        rightNumbersNumberTextBox.Text = string.Empty;
        irNumberTextBox.Text = string.Empty;
        EnLicensePlate = null;
        LoadData();
    }

    private async void DeleteLicensePlate_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dataGridRow = UIHelper.FindAncestor<DataGridRow>(button);
        var item = dataGridRow?.Item as LicensePlateListItemViewModel;
        if (item == null) return;
        var confirm = new ConfirmWindow(
            "حذف پلاک از گروه",
            $"آیا از حذف پلاک {item.Name} از این گروه اطمینان دارید؟",
            ConfirmType.Delete,
            "حذف",
            "انصراف"
        );

        if (confirm.ShowDialog() == true)
        {
            var result = await _parkingService.DeleteLicensePlate(item.Id);
            if (!result)
            {
                ShowMessage("خطا", $"خطایی رخ داده است");
                return;
            }

            ViewModel.Items.Remove(item);
            ShowMessage("موفقیت", $" پلاک {item.Name} با موفقیت حذف شد.");
        }
    }

    private void GroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedGroup = GroupFilterComboBox.SelectedItem as LicensePlateGroupModel;
        SelectedGroupId = selectedGroup?.Id;
        LoadData();
    }

    private void ClearGroupFilter_Click(object sender, RoutedEventArgs e)
    {
        SelectedGroupId = null;
        GroupFilterComboBox.SelectedItem = null;
        LoadData();
    }

    private async void ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox
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

    private async void AddLicensePlate_Click(object sender, RoutedEventArgs e)
    {
        AddLicensePlateWindow addLicensePlateWindow = new AddLicensePlateWindow();

        addLicensePlateWindow.Owner = Application.Current.MainWindow;

        if (addLicensePlateWindow.ShowDialog() == true)
        {
            var newPlate = new Domain.Entities.Vehicles.LicensePlate
            {
                EnLicensePlate = addLicensePlateWindow.EnLicensePlate,
                GroupId = addLicensePlateWindow.SelectedGroupId ?? Guid.Empty,
            };
            var result = await _parkingService.AddLicensePlate(newPlate);

            if (result.IsExsist)
            {
                ShowMessage("خطا", "امکان تعریف پلاک برای چندین گروه وجود ندارد");
                return;
            }
            if (result.IsSuccess)
            {
                ShowMessage("موفقیت", "پلاک با موفقیت به گروه اضافه شد.");
                LoadData();
            }
            else
            {
                ShowMessage("خطا", "خطایی در افزودن پلاک رخ داد.");
            }
        }
    }
}
