using Parking.Domain.Entities.Vehicles;
using System.Globalization;

namespace Parking.App.Views.Windows.LicensePlate;

/// <summary>
/// Interaction logic for EditLicensePlateGroupWindows.xaml
/// </summary>
public partial class EditLicenseGroupPlateWindow : FluentWindow
{
    private readonly PersianCalendar _pc = new PersianCalendar();

    public EditLicensePlateGroupViewModel Vm => DataContext as EditLicensePlateGroupViewModel;

    public EditLicenseGroupPlateWindow(LicensePlateGroup existingGroup)
    {
        InitializeComponent();

        var vm = new EditLicensePlateGroupViewModel(existingGroup);
        DataContext = vm;

        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.IsOptionalSelected))
            {
                OptionalDatesPanel.Visibility = vm.IsOptionalSelected
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        };

        OptionalDatesPanel.Visibility = vm.IsOptionalSelected
            ? Visibility.Visible
            : Visibility.Collapsed;
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

            DateTime start = _pc.ToDateTime(
                int.Parse(Vm.StartYear),
                int.Parse(Vm.StartMonth),
                int.Parse(Vm.StartDay),
                0, 0, 0, 0);

            DateTime end;

            if (Vm.IsOptionalSelected)
            {
                end = _pc.ToDateTime(
                    int.Parse(Vm.EndYear),
                    int.Parse(Vm.EndMonth),
                    int.Parse(Vm.EndDay),
                    0, 0, 0, 0);
            }
            else
            {
                end = _pc.ToDateTime(
                    int.Parse(Vm.EndYear),
                    int.Parse(Vm.EndMonth),
                    int.Parse(Vm.EndDay),
                    0, 0, 0, 0);
            }

            Vm.StartDate = start;
            Vm.EndDate = end;
            Vm.EndDateString = end.ToShamsi();

            DialogResult = true;
        }
        catch
        {
            DialogResult = false;
        }
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
}
