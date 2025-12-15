using System.Text.RegularExpressions;
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
    private static readonly Regex _numericRegex = new Regex("^[0-9]+$");

    private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !_numericRegex.IsMatch(e.Text);
    }

    private void NumericOnly_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
            e.Key == Key.Left || e.Key == Key.Right)
            return;

        if ((e.Key < Key.D0 || e.Key > Key.D9) &&
            (e.Key < Key.NumPad0 || e.Key > Key.NumPad9))
        {
            e.Handled = true;
        }
    }

    private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(System.Windows.DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        var text = e.DataObject.GetData(System.Windows.DataFormats.Text) as string;

        // allow only digits
        if (string.IsNullOrEmpty(text) || !_numericRegex.IsMatch(text))
            e.CancelCommand();
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

            string exitFrom = request.ExitFrom?.ToShamsi(includeTime: false) ?? "—";
            string exitTo = request.ExitTo?.ToShamsi(includeTime: false) ?? "—";

            string entryRegistrar = request.EntryRegistrar ?? "همه";
            string exitRegistrar = request.ExitRegistrar ?? "همه";

            var filterRows = new[]
            {
    "فیلترهای گزارش",
    $"خروج: از {exitFrom}   |   تا {exitTo}",
    $"ثبت‌کننده ورود: {entryRegistrar}",
    $"ثبت‌کننده خروج: {exitRegistrar}"
};

            var summaryData = new List<TicketSummaryReportModel>
        {
            ViewModel.Report
        };

            byte[] fileBytes = ExcelHelper.ExportToExcel(
                summaryData,
                "SummaryReport",
                true,
                filterRows
            );

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
        catch
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
