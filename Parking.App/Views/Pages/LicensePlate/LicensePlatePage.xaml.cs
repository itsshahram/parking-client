using Parking.App.Views.Windows.LicensePlate;
using Button = System.Windows.Controls.Button;

namespace Parking.App.Views.Pages.LicensePlate;

public partial class LicensePlatePage : Page
{
    private LicensePlateGroupViewModel ViewModel { get; set; }
    private readonly IParkingService _parkingService;
    private readonly ILogger<LicensePlatePage> _logger;
    private string? EnLicensePlate { get; set; } = null;
    private Guid? SelectedGroupId { get; set; } = null;
    private bool _isSelectingItem = false;
    private CancellationTokenSource _debounceCts;

    public LicensePlatePage()
    {
        InitializeComponent();
        ViewModel = new LicensePlateGroupViewModel();
        DataContext = ViewModel;

        _parkingService = App.GetService<IParkingService>();
        _logger = App.GetService<ILogger<LicensePlatePage>>();

        InitPlateChars();
        LoadData();

        GroupFilterComboBox.Loaded += GroupFilterComboBox_Loaded;

        _ = LoadInitialGroupFilterAsync();
    }

    private System.Windows.Controls.TextBox GetComboBoxTextBox(ComboBox comboBox)
    {
        return comboBox.Template.FindName("PART_EditableTextBox", comboBox) as System.Windows.Controls.TextBox;
    }

    private async void GroupFilterComboBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!GroupFilterComboBox.IsLoaded || !GroupFilterComboBox.IsEditable)
            return;

        if (_isSelectingItem)
        {
            _isSelectingItem = false;
            return;
        }

        string filterText = GroupFilterComboBox.Text?.Trim() ?? string.Empty;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(500, token); 

            if (string.IsNullOrWhiteSpace(filterText))
            {
                await Dispatcher.InvokeAsync(async () => await LoadInitialGroupFilterAsync());
                return;
            }

            await Dispatcher.InvokeAsync(async () => await LoadGroupFilter(filterText));
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debounce filter");
        }
    }


    private void GroupFilterComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var comboTextBox = GetComboBoxTextBox(GroupFilterComboBox);
        if (comboTextBox != null)
        {
            comboTextBox.TextChanged -= GroupFilterComboBox_TextChanged; 
            comboTextBox.TextChanged += GroupFilterComboBox_TextChanged;

            comboTextBox.KeyDown -= ComboBoxTextBox_KeyDown;
            comboTextBox.KeyDown += ComboBoxTextBox_KeyDown;
        }
    }


    private void ComboBoxTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchGroupButton_Click(sender, e);
            e.Handled = true;
        }
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

    /// <summary>
    /// Load first 20 LicensePlateGroup items when page initializes.
    /// </summary>
    private async Task LoadInitialGroupFilterAsync()
    {
        try
        {
            var data = await _parkingService.GetLicensePlateList(Page: 1, Take: 20);
            GroupFilterComboBox.ItemsSource = data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial LicensePlate groups");
        }
    }

    /// <summary>
    /// Search and filter LicensePlate groups using the service method with pagination.
    /// </summary>
    private async Task LoadGroupFilter(string filter)
    {
        try
        {
            var data = await _parkingService.GetLicensePlateList(Page: 1, Take: 20, q: filter);
            GroupFilterComboBox.ItemsSource = data;

            GroupFilterComboBox.IsDropDownOpen = data.Any();
            if (data.Any())
                GroupFilterComboBox.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering LicensePlate groups");
        }
    }



    private async void SearchGroupButton_Click(object sender, RoutedEventArgs e)
{
string? filterText = GroupFilterComboBox.Text;
await LoadGroupFilter(filterText);
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
        if (GroupFilterComboBox.SelectedItem != null)
        {
            _isSelectingItem = true;
            var selectedGroup = GroupFilterComboBox.SelectedItem as LicensePlateGroupModel;
            SelectedGroupId = selectedGroup?.Id;
            LoadData();
        }
    }

    private async void ClearGroupFilter_Click(object sender, RoutedEventArgs e)
    {
        SelectedGroupId = null;
        GroupFilterComboBox.SelectedItem = null;
        GroupFilterComboBox.Text = string.Empty;
        await LoadInitialGroupFilterAsync();
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
