namespace Parking.App.Views.Windows
{
    public partial class AddSeizedPelakWindow : FluentWindow
    {
        private readonly IParkingService _parkingService;
        private readonly Action _onPlateAdded;

        public AddSeizedPelakWindow(Action onPlateAdded = null)
        {
            _parkingService = App.GetService<IParkingService>();
            _onPlateAdded = onPlateAdded;

            InitializeComponent();
            LoadPlateChars();
        }

        private void LoadPlateChars()
        {
            var plateChars = LicensePlateHelper.GetChars();
            plateCharsCombo.ItemsSource = plateChars.Select(p => p.PlateFa).ToList();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string plate = string.Empty;

            if (SinglePlateToggle.IsChecked == true)
                plate = FullPlateTextBox.Text?.Trim();
            else
            {
                if (!string.IsNullOrEmpty(leftNumbersNumberTextBox.Text))
                    plate += leftNumbersNumberTextBox.Text;

                var plateChar = plateCharsCombo.Text;
                if (!string.IsNullOrEmpty(plateChar))
                {
                    plate += $"_{plateChar.ConvertFaCharToEnCharIndex()}";
                    if (!string.IsNullOrEmpty(rightNumbersNumberTextBox.Text))
                    {
                        plate += $"_{rightNumbersNumberTextBox.Text}";
                        if (!string.IsNullOrEmpty(irNumberTextBox.Text) && irNumberTextBox.Text.Length == 2)
                            plate += $"_IR{irNumberTextBox.Text}";
                    }
                }
            }

            var result = await _parkingService.AddSeizedVehicleAsync(plate, DescriptionTextBox.Text);
            if (result.IsSuccess)
            {
                await ShowMessage("موفق", "عملیات با موفقیت انجام شد");
                _onPlateAdded?.Invoke();
                this.Close();
                return;
            }

            if (result.Exists)
                await ShowMessage("خطا", "این پلاک در حال حاضر توقیف شده است");
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private async Task ShowMessage(string title, string message)
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
        private void SinglePlateToggle_Checked(object sender, RoutedEventArgs e)
        {
            FullPlateTextBox.Visibility = Visibility.Visible;
            SegmentedPlateGrid.Visibility = Visibility.Collapsed;
        }

        private void SinglePlateToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            FullPlateTextBox.Visibility = Visibility.Collapsed;
            SegmentedPlateGrid.Visibility = Visibility.Visible;
        }
    }
}
