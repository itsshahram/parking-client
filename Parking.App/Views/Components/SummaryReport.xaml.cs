using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Parking.App.Views.Components;

/// <summary>
/// Interaction logic for SummaryReport.xaml
/// </summary>
public partial class SummaryReport : UserControl
{
    private readonly IParkingService _parkingService;
    public SummaryReportViewModel ViewModel { get; set; }

    public SummaryReport()
    {
        InitializeComponent();
        _parkingService = App.GetService<IParkingService>();
        ViewModel = new SummaryReportViewModel();
        DataContext = ViewModel;
        SetDefaultParameter();
    }



    private void SetDefaultParameter()
    {
        var entryRegistrars = _parkingService.GetEntryRegistrars();

        var exitRegistrars = _parkingService.GetExitRegistrars();
        EntryRegistrarCombo.ItemsSource = new ObservableCollection<string>(new[] { "همه" }.Concat(entryRegistrars));

        ExitRegistrarCombo.ItemsSource = new ObservableCollection<string>(new[] { "همه" }.Concat(exitRegistrars));

    }

    private GetTicketListRequestModel FillParameters()
    {
        var request = new GetTicketListRequestModel();

        if (!string.IsNullOrWhiteSpace(exitStartYearTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartMonthTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartDayTextBox.Text))
        {
            request.ExitFrom = DateConvertor.ShamsiToDateTime(
                int.Parse(exitStartYearTextBox.Text),
                int.Parse(exitStartMonthTextBox.Text),
                int.Parse(exitStartDayTextBox.Text));

            if (!string.IsNullOrWhiteSpace(exitEndYearTextBox.Text) &&
                !string.IsNullOrWhiteSpace(exitEndMonthTextBox.Text) &&
                !string.IsNullOrWhiteSpace(exitEndDayTextBox.Text))
            {
                request.ExitTo = DateConvertor.ShamsiToDateTime(
                    int.Parse(exitEndYearTextBox.Text),
                    int.Parse(exitEndMonthTextBox.Text),
                    int.Parse(exitEndDayTextBox.Text));
            }

            request.EntryRegistrar = EntryRegistrarCombo.Text == "همه" ? null : EntryRegistrarCombo.Text;
            request.ExitRegistrar = ExitRegistrarCombo.Text == "همه" ? null : ExitRegistrarCombo.Text;

        }
        return request;
    }

    private void SetFieldsEnabled(bool isEnabled)
    {
        var textBoxes = new[]
        {
        exitStartYearTextBox, exitStartMonthTextBox, exitStartDayTextBox,
        exitEndYearTextBox, exitEndMonthTextBox, exitEndDayTextBox,
    };

        foreach (var tb in textBoxes)
            tb.IsEnabled = isEnabled;

        var comboBoxes = new[]
        {
        EntryRegistrarCombo,
        ExitRegistrarCombo
    };


        foreach (var cb in comboBoxes)
            cb.IsEnabled = isEnabled;

        var buttons = new[]
        {
            ExportButton, SearchBtn
        };
        foreach (var btn in buttons)
            btn.IsEnabled = isEnabled;
    }


    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            progressBar.IsIndeterminate = true;
            SetFieldsEnabled(false);
            var request = FillParameters();

            var filterDescription = $"خروج از: {request.ExitFrom?.ToShamsi(includeTime: false) ?? "-"} " +
                                    $"تا: {request.ExitTo?.ToShamsi(includeTime: false) ?? "-"}, " +
                                    $"ثبت ‌کننده ورود: {request.EntryRegistrar ?? "همه"}, " +
                                    $"ثبت‌ کننده خروج: {request.ExitRegistrar ?? "همه"}";


            var summaryData = new List<TicketSummaryReportModel>
        {
            ViewModel.Report
        };


            byte[] fileBytes = ExcelHelper.ExportToExcel(summaryData, "SummaryReport", true, filterDescription);

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "SummaryReport.xlsx"
            };
            progressBar.IsIndeterminate = false;
            SetFieldsEnabled(true);

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, fileBytes);
                ShowMessage("موفق", "فایل با موفقیت ذخیره شد.");
            }
        }
        catch (Exception)
        {
            progressBar.IsIndeterminate = false;
            SetFieldsEnabled(true);
            ShowMessage("خطا", "خطایی رخ داد.");
        }
    }

    private async void SearchBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetFieldsEnabled(false);
            progressBar.IsIndeterminate = true;

            var request = FillParameters();
            var newReport = await _parkingService.GetSummaryReport(request);


            ViewModel.Report = newReport;
        }
        finally
        {
            progressBar.IsIndeterminate = false;
            SetFieldsEnabled(true);
        }
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        // تاریخ خروج از
        exitStartYearTextBox.Text = "";
        exitStartMonthTextBox.Text = "";
        exitStartDayTextBox.Text = "";

        // تاریخ خروج تا
        exitEndYearTextBox.Text = "";
        exitEndMonthTextBox.Text = "";
        exitEndDayTextBox.Text = "";

        // ComboBoxes reset
        EntryRegistrarCombo.SelectedIndex = 0;
        ExitRegistrarCombo.SelectedIndex = 0;
    }

    private async void ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    MessageBox ms = new MessageBox();
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
