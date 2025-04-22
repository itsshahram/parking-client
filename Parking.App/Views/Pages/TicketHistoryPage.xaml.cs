using Azure.Core;
using Microsoft.Extensions.Logging;
using Parking.App.Helpers;
using Parking.App.Models;
using Parking.App.Models.Dto.Parking.ParkingTicket;
using Parking.App.Models.Dto.Vehicle.LicensePlate;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.Tickets;
using Parking.App.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


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
            SetDefaultParameter();
        }

        private void SetDefaultParameter()
        {
            var yesterdayTime = DateTime.Now.AddDays(-1).ToShamsi().Split(" / ");
            entryStartYearTextBox.Text = yesterdayTime[0];
            entryStartMountTextBox.Text = yesterdayTime[1];
            entryStartDayTextBox.Text = yesterdayTime[2];

            var nowTime = DateTime.Now.ToShamsi().Split(" / ");
            entryEndYearTextBox.Text = nowTime[0];
            entryEndMountTextBox.Text = nowTime[1];
            entryEndDayTextBox.Text = nowTime[2];
            entryEndHourTextBox.Text = DateTime.Now.Hour.ToString();
            entryEndMinutesTextBox.Text = DateTime.Now.Minute.ToString();


            List<VehicleSegmentModel> list = new List<VehicleSegmentModel>();
            list.Add(new VehicleSegmentModel { Id = 0, NameFa = "همه" });
            list.AddRange(_parkingService.GetVehicleSegments());
            vehicleSegmentList.ItemsSource = new ObservableCollection<VehicleSegmentModel>(list);

        }
        private void CheckParameter()
        {
            Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
            ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            if (entryStartYearTextBox.Text == null || entryStartYearTextBox.Text.Length<4)
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد سال را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartMountTextBox.Text == null )
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد ماه را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartDayTextBox.Text == null )
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ ابتدا را چک کیند. (فیلد روز را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartHourTextBox.Text == null )
            {
                ms.Title = "خطا";
                ms.Content = "لطفا ساعت و دقیقه ابتدا را چک کنید";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryStartMinutesTextBox.Text == null )
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
            if (entryEndMountTextBox.Text == null )
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد ماه را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndDayTextBox.Text == null )
            {
                ms.Title = "خطا";
                ms.Content = "لطفا تاریخ انتهایی را چک کیند. (فیلد روز را وارد نکرده اید)";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                ms.ShowDialogAsync();
                return;
            }
            if (entryEndHourTextBox.Text == null )
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
                DateTime start = DateConvertor.ShamsiToDateTime(
                    int.Parse(entryStartYearTextBox.Text.ToString()),
                    int.Parse(entryStartMountTextBox.Text.ToString()),
                    int.Parse(entryStartDayTextBox.Text.ToString()),
                    int.Parse(entryStartHourTextBox.Text.ToString()),
                    int.Parse(entryStartMinutesTextBox.Text.ToString())
                    );
                request.StartStartTime = start;
                DateTime end = DateConvertor.ShamsiToDateTime(
                    int.Parse(entryEndYearTextBox.Text.ToString()),
                    int.Parse(entryEndMountTextBox.Text.ToString()),
                    int.Parse(entryEndDayTextBox.Text.ToString()),
                    int.Parse(entryEndHourTextBox.Text.ToString()),
                    int.Parse(entryEndMinutesTextBox.Text.ToString())
                    );
                request.EndStartTime = end;
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
        private async void TicketsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ticketsDataGrid.SelectedItem is TicketsListViewModel selectedItem)
            {
                var ticket = ticketsDataGrid.SelectedItem as TicketsListViewModel;
                var Details = new TicketDetailsWindow(ticket.Id, null, null, null);
                Details?.Show();
            }
        }
    }
}
