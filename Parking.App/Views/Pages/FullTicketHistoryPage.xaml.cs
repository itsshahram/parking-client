using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Parking.App.Views.Pages;

/// <summary>
/// Interaction logic for FullTicketHistoryPage.xaml
/// </summary>
public partial class FullTicketHistoryPage : Page
{
    private readonly ILogger<TicketHistoryPage>? _logger;
    private readonly IParkingService? _parkingService;
    private GetTicketListRequestModel ViewModel { get; set; }
    public FullTicketHistoryPage()
    {
        InitializeComponent();

        _logger = App.GetService<ILogger<TicketHistoryPage>>();
        _parkingService = App.GetService<IParkingService>();
        ViewModel = new GetTicketListRequestModel();
        DataContext = ViewModel;

        LoadData();

        var window = Window.GetWindow(this);
        if (window != null)
            window.WindowState = WindowState.Minimized;

        SetDefaultParameter();
    }

    private void LoadData()
    {
        GetTicketListRequestModel request = FillParameters();
        var tickets = _parkingService.GetTicketList(request);
        ViewModel.Items = new ObservableCollection<TicketsListViewModel>(tickets.Data);
        ViewModel.CurrentPage = 1;
        ViewModel.ItemsPerPage = 10;
        ViewModel.TotalCount = tickets.TotalCount;
        ticketsDataGrid.ItemsSource = new ObservableCollection<TicketsListViewModel>(tickets.Data);
    }

    private void PaginationControl_PageChanged(object sender, int newPage)
    {
        GetTicketListRequestModel request = FillParameters();
        ViewModel.CurrentPage = newPage;
        request.CurrentPage = newPage;
        var tickets = _parkingService.GetTicketList(request);
        resultCount.Text = tickets.TotalCount.ToString("N0");
        ViewModel.ItemsPerPage = 10;
        ViewModel.TotalCount = tickets.TotalCount;
        ticketsDataGrid.ItemsSource = new ObservableCollection<TicketsListViewModel>(tickets.Data);
    }
    private void PaginationControl_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void SetDefaultParameter()
    {
        List<VehicleSegmentModel> list = [new VehicleSegmentModel { Id = 0, NameFa = "همه" }, .. _parkingService.GetVehicleSegments()];

        var entryRegistrars = _parkingService.GetEntryRegistrars();

        var exitRegistrars = _parkingService.GetExitRegistrars();
        EntryRegistrarCombo.ItemsSource = new ObservableCollection<string>(new[] { "همه" }.Concat(entryRegistrars));

        ExitRegistrarCombo.ItemsSource = new ObservableCollection<string>(new[] { "همه" }.Concat(exitRegistrars));

        vehicleSegmentList.ItemsSource = new ObservableCollection<VehicleSegmentModel>(list);
    }
    private void CheckParameter()
    {
        var allFields = new[]
        {
               entryStartYearTextBox.Text,
               entryStartMountTextBox.Text,
               entryStartDayTextBox.Text,
               entryStartHourTextBox.Text,
               entryStartMinutesTextBox.Text,
               entryEndYearTextBox.Text,
               entryEndMountTextBox.Text,
               entryEndDayTextBox.Text,
               entryEndHourTextBox.Text,
               entryEndMinutesTextBox.Text
               };

        // if all fields is empty do nothing
        if (allFields.All(string.IsNullOrWhiteSpace))
            return;

        Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
        ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;

        if (!string.IsNullOrWhiteSpace(entryStartYearTextBox.Text) && entryStartYearTextBox.Text.Length < 4)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد سال کمتر از 4 رقم است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryStartMountTextBox.Text) && entryStartMountTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد ماه ناقص است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryStartDayTextBox.Text) && entryStartDayTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد روز ناقص است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryStartHourTextBox.Text) && entryStartHourTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا ساعت ابتدا را چک کنید (دو رقم وارد کنید)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryStartMinutesTextBox.Text) && entryStartMinutesTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا دقیقه ابتدا را چک کنید (دو رقم وارد کنید)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(entryEndYearTextBox.Text) && entryEndYearTextBox.Text.Length < 4)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد سال ناقص است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryEndMountTextBox.Text) && entryEndMountTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد ماه ناقص است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryEndDayTextBox.Text) && entryEndDayTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد روز ناقص است)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryEndHourTextBox.Text) && entryEndHourTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا ساعت انتهایی را چک کنید (دو رقم وارد کنید)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
        if (!string.IsNullOrWhiteSpace(entryEndMinutesTextBox.Text) && entryEndMinutesTextBox.Text.Length < 2)
        {
            ms.Title = "خطا";
            ms.Content = "لطفا دقیقه انتهایی را چک کنید (دو رقم وارد کنید)";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
    }

    private async void SearchBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            progressBar.IsIndeterminate = true;

            CheckParameter();
            GetTicketListRequestModel request = FillParameters();

            var tickets = await _parkingService.GetTicketListAsync(request);
            ViewModel.Items = new ObservableCollection<TicketsListViewModel>(tickets.Data);
            ViewModel.CurrentPage = 1;
            ViewModel.ItemsPerPage = 10;
            ViewModel.TotalCount = tickets.TotalCount;
            ticketsDataGrid.ItemsSource = new ObservableCollection<TicketsListViewModel>(tickets.Data);
            resultCount.Text = tickets.TotalCount.ToString("N0");
            progressBar.IsIndeterminate = false;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
            ms.Title = "خطا";
            ms.Content = "خطا در دریافت اطلاعات";
            ms.IsPrimaryButtonEnabled = false;
            ms.IsSecondaryButtonEnabled = false;
            ms.CloseButtonText = "متوجه شدم";
            ms.ShowDialogAsync();
            return;
        }
    }

    private GetTicketListRequestModel FillParameters()
    {
        GetTicketListRequestModel request = new GetTicketListRequestModel();

        // ورود از
        if (!string.IsNullOrWhiteSpace(entryStartYearTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryStartMountTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryStartDayTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryStartHourTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryStartMinutesTextBox.Text))
        {
            request.EntryFrom = DateConvertor.ShamsiToDateTime(
                int.Parse(entryStartYearTextBox.Text),
                int.Parse(entryStartMountTextBox.Text),
                int.Parse(entryStartDayTextBox.Text),
                int.Parse(entryStartHourTextBox.Text),
                int.Parse(entryStartMinutesTextBox.Text));
        }

        // ورود تا
        if (!string.IsNullOrWhiteSpace(entryEndYearTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryEndMountTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryEndDayTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryEndHourTextBox.Text) &&
            !string.IsNullOrWhiteSpace(entryEndMinutesTextBox.Text))
        {
            request.EntryTo = DateConvertor.ShamsiToDateTime(
                int.Parse(entryEndYearTextBox.Text),
                int.Parse(entryEndMountTextBox.Text),
                int.Parse(entryEndDayTextBox.Text),
                int.Parse(entryEndHourTextBox.Text),
                int.Parse(entryEndMinutesTextBox.Text));
        }

        // خروج از
        if (!string.IsNullOrWhiteSpace(exitStartYearTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartMountTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartDayTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartHourTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitStartMinutesTextBox.Text))
        {
            request.ExitFrom = DateConvertor.ShamsiToDateTime(
                int.Parse(exitStartYearTextBox.Text),
                int.Parse(exitStartMountTextBox.Text),
                int.Parse(exitStartDayTextBox.Text),
                int.Parse(exitStartHourTextBox.Text),
                int.Parse(exitStartMinutesTextBox.Text));
        }

        // خروج تا
        if (!string.IsNullOrWhiteSpace(exitEndYearTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitEndMountTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitEndDayTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitEndHourTextBox.Text) &&
            !string.IsNullOrWhiteSpace(exitEndMinutesTextBox.Text))
        {
            request.ExitTo = DateConvertor.ShamsiToDateTime(
                int.Parse(exitEndYearTextBox.Text),
                int.Parse(exitEndMountTextBox.Text),
                int.Parse(exitEndDayTextBox.Text),
                int.Parse(exitEndHourTextBox.Text),
                int.Parse(exitEndMinutesTextBox.Text));
        }
        // وضعیت پرداخت
        if (isPaid.SelectedItem is ComboBoxItem selectedItem &&
            bool.TryParse(selectedItem.Tag?.ToString(), out bool result))
        {
            request.IsPaid = result;
        }

        // نوع پرداخت
        request.PaidType = Payment_Type.Text == "همه"
            ? null
            : (Payment_Type.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        // نوع درگاه
        request.GateType = GateType.Text == "همه"
            ? null
            : (GateType.SelectedItem as ComboBoxItem)?.Tag?.ToString();


        var selectedVehicleStatus = VehicleStatus.SelectedItem as ComboBoxItem;

        request.VehicleStatus = (selectedVehicleStatus != null && selectedVehicleStatus.Content.ToString() != "همه")
            ? selectedVehicleStatus.Tag.ToString() switch
            {
                "0" => Domain.General.VehicleStatus.Entered,
                "1" => Domain.General.VehicleStatus.Exited,
                _ => null
            }
            : null;

        // پلاک
        string plate = "";
        if (!string.IsNullOrWhiteSpace(leftNumbersNumberTextBox.Text))
            plate += leftNumbersNumberTextBox.Text;

        if (!string.IsNullOrWhiteSpace(plateCharacter.Text))
        {
            plate += $"_{plateCharacter.Text.ConvertFaCharToEnCharIndex()}";

            if (!string.IsNullOrWhiteSpace(rightNumbersNumberTextBox.Text))
            {
                plate += $"_{rightNumbersNumberTextBox.Text}";

                if (!string.IsNullOrWhiteSpace(irNumberTextBox.Text) && irNumberTextBox.Text.Length == 2)
                    plate += $"_IR{irNumberTextBox.Text}";
            }
        }

        if (!string.IsNullOrEmpty(plate))
            request.LicensePlate = plate;

        // سگمنت خودرو
        if (vehicleSegmentList.SelectedItem is VehicleSegmentModel segment)
            request.VehicleSegmentId = segment.Id == 0 ? null : segment.Id;

        if (!string.IsNullOrEmpty(PriceFrom.Text))
            request.PriceFrom = int.Parse(PriceFrom.Text);

        if (!string.IsNullOrEmpty(PriceTo.Text))
            request.PriceTo = int.Parse(PriceTo.Text);

        request.EntryRegistrar = EntryRegistrarCombo.Text == "همه" ? null : EntryRegistrarCombo.Text;
        request.ExitRegistrar = ExitRegistrarCombo.Text == "همه" ? null : ExitRegistrarCombo.Text;
        request.HasDiscrepancy = DiscrepancyCheckBox.IsChecked == true ? true : null;
        request.RRN = RRNTextBox.Text;
        request.TrackNo = TrackNoTextBox.Text;

        return request;
    }
    private string BuildFilterDescription(GetTicketListRequestModel request)
    {
        var filters = new List<string>();

        if (request.EntryFrom != null || request.EntryTo != null)
            filters.Add($"ورود از {request.EntryFrom?.ToString("yyyy/MM/dd HH:mm") ?? "-"} تا {request.EntryTo?.ToString("yyyy/MM/dd HH:mm") ?? "-"}");

        if (request.ExitFrom != null || request.ExitTo != null)
            filters.Add($"خروج از {request.ExitFrom?.ToString("yyyy/MM/dd HH:mm") ?? "-"} تا {request.ExitTo?.ToString("yyyy/MM/dd HH:mm") ?? "-"}");

        if (request.PriceFrom != null || request.PriceTo != null)
            filters.Add($"مبلغ از {request.PriceFrom?.ToString("#,0")} تا {request.PriceTo?.ToString("#,0")} ریال");

        if (request.IsPaid != null)
            filters.Add($"وضعیت پرداخت: {(request.IsPaid == true ? "پرداخت شده" : "پرداخت نشده")}");

        if (!string.IsNullOrEmpty(request.PaidType))
            filters.Add($"نوع پرداخت:{(request.PaidType == "Naghdi" ? "پرداخت شده" : "پرداخت نشده")}");

        if (!string.IsNullOrEmpty(request.GateType))
            filters.Add($"نوع درگاه: {request.GateType}");

        if (request.VehicleStatus != null)
            filters.Add($"وضعیت خودرو: {(request.VehicleStatus == Domain.General.VehicleStatus.Entered ? "داخل" : "خارج شده")}");

        if (!string.IsNullOrEmpty(request.LicensePlate))
            filters.Add($"پلاک: {request.LicensePlate}");


        if (request.VehicleSegmentId != null)
            filters.Add($"سگمنت خودرو: {request.VehicleSegmentId}");

        if (!string.IsNullOrEmpty(request.EntryRegistrar))
            filters.Add($"ثبت‌کننده ورود: {request.EntryRegistrar}");

        if (!string.IsNullOrEmpty(request.ExitRegistrar))
            filters.Add($"ثبت‌کننده خروج: {request.ExitRegistrar}");

        if (request.HasDiscrepancy != null && request.HasDiscrepancy == true)
            filters.Add("فقط با مغایرت");

        return filters.Count > 0 ? string.Join(" | ", filters) : "بدون فیلتر";
    }

    private async void TicketsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ticketsDataGrid.SelectedItem is TicketsListViewModel selectedItem)
        {
            var window = Window.GetWindow(this);
            var ticket = ticketsDataGrid.SelectedItem as TicketsListViewModel;
            var Details = new TicketDetailsWindow(ticket.Id, null, null, null);
            Details.Owner = window;
            Details?.ShowDialog();
        }
    }

    private void vehicleSegmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void GateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        // تاریخ ورود از
        entryStartYearTextBox.Text = "";
        entryStartMountTextBox.Text = "";
        entryStartDayTextBox.Text = "";
        entryStartHourTextBox.Text = "";
        entryStartMinutesTextBox.Text = "";

        // تاریخ ورود تا
        entryEndYearTextBox.Text = "";
        entryEndMountTextBox.Text = "";
        entryEndDayTextBox.Text = "";
        entryEndHourTextBox.Text = "";
        entryEndMinutesTextBox.Text = "";

        // تاریخ خروج از
        exitStartYearTextBox.Text = "";
        exitStartMountTextBox.Text = "";
        exitStartDayTextBox.Text = "";
        exitStartHourTextBox.Text = "";
        exitStartMinutesTextBox.Text = "";

        // تاریخ خروج تا
        exitEndYearTextBox.Text = "";
        exitEndMountTextBox.Text = "";
        exitEndDayTextBox.Text = "";
        exitEndHourTextBox.Text = "";
        exitEndMinutesTextBox.Text = "";

        // پلاک
        leftNumbersNumberTextBox.Text = "";
        plateCharacter.Text = "";
        rightNumbersNumberTextBox.Text = "";
        irNumberTextBox.Text = "";

        // ComboBoxes reset
        vehicleSegmentList.SelectedIndex = 0;
        GateType.SelectedIndex = 0;
        Payment_Type.SelectedIndex = 0;
        isPaid.SelectedIndex = 0;
        PriceTo.Text = "";
        PriceFrom.Text = "";
        DiscrepancyCheckBox.IsChecked = false;

        RRNTextBox.Text = "";
        TrackNoTextBox.Text = "";
    }

    private void VehicleStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }
    private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }
    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        GetTicketListRequestModel request = FillParameters();

        string filterDescription = BuildFilterDescription(request);
        var report = await _parkingService.GetTicketListReportAsync(request);

        var data = report.Data.Select(x => new TicketSummaryReportItem()
        {
            StartTime = x.StartTime.ToShamsi(),
            EndTime = x.EndTime?.ToShamsi(),
            LicensePlate = x.LicensePlate,
            VehicleSegmentName = x.VehicleSegmentName,
            ParkingName = x.ParkingName,
            EntranceGate = x.EntranceGate,
            ExitGate = x.ExitGate,
            DurationMinutes = x.DurationMinutes,
            TotalAmount = x.TotalAmount,
            Discount = x.Discount,
            PaidAmount = x.PaidAmount,
            PaidType = x.PaidType == "Naghdi" ? "نقدی" : "پوز",
            PaidCreditCard = x.PaidCreditCard,
            IsPaid = x.IsPaid.Value == true ? "پرداخت شده" : "پرداخت نشده",
            IsExited = x.IsExited.Value == true ? "خارج شده" : "وارد شده"
        }).ToList();


        byte[] fileBytes = ExcelHelper.ExportToExcel(data, "FullReport", true, filterDescription);

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = "FulReport.xlsx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            File.WriteAllBytes(saveFileDialog.FileName, fileBytes);
            ShowMessage("موفق", "فایل با موفقیت ذخیره شد.");
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
