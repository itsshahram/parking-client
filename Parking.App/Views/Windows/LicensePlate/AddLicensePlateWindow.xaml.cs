namespace Parking.App.Views.Windows.LicensePlate;

/// <summary>
/// Interaction logic for AddLicensePlateWindow.xaml
/// </summary>
public partial class AddLicensePlateWindow : FluentWindow
{
    private readonly IParkingService _parkingService;
    private readonly ILogger<AddLicensePlateWindow> _logger;
    private readonly Guid? _defaultGroupId;
    private readonly bool _lockGroupSelection;

    public Guid? SelectedGroupId { get; private set; }
    public string? EnLicensePlate { get; private set; }

    public AddLicensePlateWindow(Guid? defaultGroupId = null, bool lockGroupSelection = false)
    {
        InitializeComponent();
        _parkingService = App.GetService<IParkingService>();
        _logger = App.GetService<ILogger<AddLicensePlateWindow>>();
        _defaultGroupId = defaultGroupId;
        _lockGroupSelection = lockGroupSelection;
        LoadGroups();
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

    private async void LoadGroups()
    {
        try
        {
            var data = await _parkingService.GetLicensePlateList();
            GroupComboBox.ItemsSource = data;
            if (_defaultGroupId.HasValue)
            {
                var selectedGroup = data.FirstOrDefault(g => g.Id == _defaultGroupId.Value);
                if (selectedGroup != null)
                {
                    GroupComboBox.SelectedItem = selectedGroup;
                }
            }
            GroupComboBox.IsEnabled = !_lockGroupSelection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading license plate groups");
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedGroup = GroupComboBox.SelectedItem as LicensePlateGroupModel;
        if (selectedGroup == null)
        {
            ShowMessage("خطا", "لطفاً یک گروه انتخاب کنید.");
            return;
        }
        if (string.IsNullOrWhiteSpace(leftNumbersNumberTextBox.Text) ||
            string.IsNullOrWhiteSpace(rightNumbersNumberTextBox.Text) ||
            string.IsNullOrWhiteSpace(irNumberTextBox.Text) ||
            plateCharsCombo.SelectedItem == null)
        {
            ShowMessage("خطا", "لطفاً تمام قسمت‌های پلاک را تکمیل کنید.");
            return;
        }

        var plateChar = (plateCharsCombo.SelectedItem as string);
        EnLicensePlate = $"{leftNumbersNumberTextBox.Text}_{plateChar?.ConvertFaCharToEnCharIndex()}_{rightNumbersNumberTextBox.Text}_IR{irNumberTextBox.Text}";
        SelectedGroupId = selectedGroup.Id;

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
            await Dispatcher.Invoke(async () =>
            {
                Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox
                {
                    FlowDirection = System.Windows.FlowDirection.RightToLeft,
                    Title = title,
                    Content = message,
                    IsPrimaryButtonEnabled = false,
                    IsSecondaryButtonEnabled = false,
                    CloseButtonText = "باشه"
                };
                await ms.ShowDialogAsync();
            });
        }
        catch
        {
            System.Windows.MessageBox.Show(message, title);
        }
    }
}
