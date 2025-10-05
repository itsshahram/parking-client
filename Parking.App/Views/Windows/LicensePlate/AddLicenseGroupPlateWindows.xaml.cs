using Parking.Domain.General;
using System.Globalization;

namespace Parking.App.Views.Windows.LicensePlate;

/// <summary>
/// Interaction logic for AddLicensePlateWindows.xaml
/// </summary>
public partial class AddLicenseGroupPlateWindows : FluentWindow
{
    private PersianCalendar _pc = new PersianCalendar();

    public AddLicensePlateGroupViewModel Vm => DataContext as AddLicensePlateGroupViewModel;

    public AddLicenseGroupPlateWindows()
    {
        InitializeComponent();
        DataContext = new AddLicensePlateGroupViewModel();

        Vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Vm.IsOptionalSelected))
            {
                OptionalDatesPanel.Visibility = Vm.IsOptionalSelected
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (!Vm.IsOptionalSelected)
                {
                    var start = GetStartDateFromViewModel();
                    DateTime end = Vm.GroupDate switch
                    {
                        LicensePlateGroupDate.OneMonth => start.AddMonths(1),
                        LicensePlateGroupDate.ThreeMonth => start.AddMonths(3),
                        LicensePlateGroupDate.SixMonth => start.AddMonths(6),
                        _ => start.AddMonths(1)
                    };

                    SetEndDate(end);
                }
            }
        };
    }

    private void PercentBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Vm.Validate(out string errorMessage))
            {
                ShowMessage("خطا", errorMessage);
                return;
            }

            DateTime start = GetStartDateFromViewModel();
            DateTime end;

            if (Vm.IsOptionalSelected)
            {
                end = _pc.ToDateTime(
                    int.Parse(Vm.EndYear),
                    int.Parse(Vm.EndMonth),
                    int.Parse(Vm.EndDay),
                    int.Parse(Vm.EndHour),
                    int.Parse(Vm.EndMinute),
                    0, 0);
            }
            else
            {
                end = GetEndDateFromViewModel();
            }

            Vm.StartDate = start;
            Vm.EndDate = end;
        }
        catch
        {
            DialogResult = false;
            return;
        }
        DialogResult = true;
    }

    private DateTime GetStartDateFromViewModel()
    {
        return _pc.ToDateTime(
            int.Parse(Vm.StartYear),
            int.Parse(Vm.StartMonth),
            int.Parse(Vm.StartDay),
            int.Parse(Vm.StartHour),
            int.Parse(Vm.StartMinute),
            0, 0);
    }

    private DateTime GetEndDateFromViewModel()
    {
        return _pc.ToDateTime(
            int.Parse(Vm.EndYear),
            int.Parse(Vm.EndMonth),
            int.Parse(Vm.EndDay),
            int.Parse(Vm.EndHour),
            int.Parse(Vm.EndMinute),
            0, 0);
    }

    private void SetEndDate(DateTime end)
    {
        Vm.EndYear = _pc.GetYear(end).ToString();
        Vm.EndMonth = _pc.GetMonth(end).ToString("00");
        Vm.EndDay = _pc.GetDayOfMonth(end).ToString("00");
        Vm.EndHour = end.Hour.ToString("00");
        Vm.EndMinute = end.Minute.ToString("00");

        Vm.EndDate = end;
        Vm.EndDateString = end.ToShamsi(); 
    }
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
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