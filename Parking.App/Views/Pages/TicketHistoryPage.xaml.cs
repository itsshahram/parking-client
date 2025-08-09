using Parking.App.Models.Dto.Vehicle.VehicleSegment;


namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for TicketHistory.xaml
    /// </summary>
    public partial class TicketHistoryPage : Page
    {
        private readonly ILogger<TicketHistoryPage>? _logger;
        private readonly IParkingService? _parkingService;
        public TicketHistoryPage()
        {
            _logger = App.GetService<ILogger<TicketHistoryPage>>();
            _parkingService = App.GetService<IParkingService>();
            InitializeComponent();
            var window = Window.GetWindow(this);
            if (window != null)
                window.WindowState = WindowState.Minimized;

            SetDefaultParameter();
        }

        private void SetDefaultParameter()
        {
            var yesterday = DateTime.Now.AddDays(-1).ToShamsi().Split(" / ");
            entryStartYearTextBox.Text = yesterday[0];
            entryStartMountTextBox.Text = yesterday[1];
            entryStartDayTextBox.Text = yesterday[2];
            entryStartHourTextBox.Text = "00";
            entryStartMinutesTextBox.Text = "00";


            var now = DateTime.Now;
            var nowShamsi = now.ToShamsi().Split(" / ");
            entryEndYearTextBox.Text = nowShamsi[0];
            entryEndMountTextBox.Text = nowShamsi[1];
            entryEndDayTextBox.Text = nowShamsi[2];
            entryEndHourTextBox.Text = "23";
            entryEndMinutesTextBox.Text = "59";

            var tomorrow = now.AddDays(1).ToShamsi().Split(" / ");
            exitStartYearTextBox.Text = tomorrow[0];
            exitStartMountTextBox.Text = tomorrow[1];
            exitStartDayTextBox.Text = tomorrow[2];
            exitStartHourTextBox.Text = "00";
            exitStartMinutesTextBox.Text = "00";

            exitEndYearTextBox.Text = tomorrow[0];
            exitEndMountTextBox.Text = tomorrow[1];
            exitEndDayTextBox.Text = tomorrow[2];
            exitEndHourTextBox.Text = "23";
            exitEndMinutesTextBox.Text = "59";

            List<VehicleSegmentModel> list = [new VehicleSegmentModel { Id = 0, NameFa = "همه" }, .. _parkingService.GetVehicleSegments()];


            vehicleSegmentList.ItemsSource = new ObservableCollection<VehicleSegmentModel>(list);
        }
        private void CheckParameter()
        {
            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
            ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            if (entryStartYearTextBox.Text == null || entryStartYearTextBox.Text.Length < 4)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد سال را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartMountTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد ماه را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartDayTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد روز را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartHourTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا ساعت و دقیقه ابتدا را چک کنید";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartMinutesTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا ساعت و دقیقه ابتدا را چک کنید";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }


            if (entryEndYearTextBox.Text == null || entryEndYearTextBox.Text.Length < 4)
            {

                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد سال را وارد نکرده اید(";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndMountTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد ماه را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndDayTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد روز را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndHourTextBox.Text == null)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا ساعت و دقیقه انتهایی را چک کنید";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndMinutesTextBox.Text == null || entryEndMinutesTextBox.Text.Length < 2)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا ساعت و دقیقه انتهایی را چک کنید";
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
                GetTicketListRequestModel request = new GetTicketListRequestModel();
                DateTime entryFrom = DateConvertor.ShamsiToDateTime(
                    int.Parse(entryStartYearTextBox.Text.ToString()),
                    int.Parse(entryStartMountTextBox.Text.ToString()),
                    int.Parse(entryStartDayTextBox.Text.ToString()),
                    int.Parse(entryStartHourTextBox.Text.ToString()),
                    int.Parse(entryStartMinutesTextBox.Text.ToString())
                    );
                request.EntryFrom = entryFrom;

                DateTime entryTo = DateConvertor.ShamsiToDateTime(
                    int.Parse(entryEndYearTextBox.Text.ToString()),
                    int.Parse(entryEndMountTextBox.Text.ToString()),
                    int.Parse(entryEndDayTextBox.Text.ToString()),
                    int.Parse(entryEndHourTextBox.Text.ToString()),
                    int.Parse(entryEndMinutesTextBox.Text.ToString())
                    );

                request.EntryTo = entryTo;

                DateTime exitFrom = DateConvertor.ShamsiToDateTime(
                    int.Parse(exitStartYearTextBox.Text.ToString()),
                    int.Parse(exitStartMountTextBox.Text.ToString()),
                    int.Parse(exitStartDayTextBox.Text.ToString()),
                    int.Parse(exitStartHourTextBox.Text.ToString()),
                    int.Parse(exitStartMinutesTextBox.Text.ToString())
                    );
                request.ExitFrom = exitFrom;

                DateTime exitTo = DateConvertor.ShamsiToDateTime(
                    int.Parse(exitEndYearTextBox.Text.ToString()),
                    int.Parse(exitEndMountTextBox.Text.ToString()),
                    int.Parse(exitEndDayTextBox.Text.ToString()),
                    int.Parse(exitEndHourTextBox.Text.ToString()),
                    int.Parse(exitEndMinutesTextBox.Text.ToString()));

                request.ExitTo = exitTo;

                bool? paymentStatus = null;
                if (isPaid.SelectedItem is ComboBoxItem selectedItem && bool.TryParse(selectedItem.Tag?.ToString(), out bool result))
                    paymentStatus = result;
                request.IsPaid = paymentStatus;
                request.PaidType = Payment_Type.Text == "همه" ? null : (Payment_Type.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                request.GateType = GateType.Text == "همه" ? null : (GateType.SelectedItem as ComboBoxItem)?.Tag?.ToString();

                string plate = "";
                if (leftNumbersNumberTextBox.Text != null)
                {
                    if (leftNumbersNumberTextBox.Text.Length > 0)
                    {
                        plate += $"{leftNumbersNumberTextBox.Text}";
                    }
                }
                var plateChar = plateCharacter.Text;
                if (plateChar?.Length > 0)
                {
                    plate += $"_{plateChar?.ConvertFaCharToEnCharIndex()}";
                    if (rightNumbersNumberTextBox.Text != null)
                    {
                        plate += $"_{rightNumbersNumberTextBox.Text}";
                        if (irNumberTextBox.Text != null && irNumberTextBox.Text.Length == 2)
                            plate += $"_IR{irNumberTextBox.Text}";
                    }
                }
                request.LicensePlate = plate;
                if (vehicleSegmentList.SelectedItem != null)
                {
                    request.VehicleSegmentId = (vehicleSegmentList.SelectedItem as VehicleSegmentModel).Id;
                    if (request.VehicleSegmentId == 0)
                    {
                        request.VehicleSegmentId = null;
                    }
                }

                var tickets = await _parkingService.GetTicketListAsync(request);
                resultCount.Text = tickets.Count.ToString("N0");
                ticketsDataGrid.ItemsSource = new ObservableCollection<TicketsListViewModel>(tickets);
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
        private async void TicketsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ticketsDataGrid.SelectedItem is TicketsListViewModel selectedItem)
            {
                var ticket = ticketsDataGrid.SelectedItem as TicketsListViewModel;
                var Details = new TicketDetailsWindow(ticket.Id, null, null, null);
                Details?.Show();
            }
        }

        private void vehicleSegmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
