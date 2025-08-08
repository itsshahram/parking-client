using Parking.App.Models.Dto.Card;

namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddCardPage.xaml
    /// </summary>
    public partial class AddCardPage : Page
    {
        private AddCardPageViewModel ViewModel = new AddCardPageViewModel();
        private readonly IParkingService _parkingService;
        private readonly ILogger<AddCardPage> _logger;
        public AddCardPage()
        {
            _parkingService = App.GetService<IParkingService>();
            _logger = App.GetService<ILogger<AddCardPage>>();

            this.DataContext = ViewModel;
            InitializeComponent();
            var vehiclecount = _parkingService.GetVehicleSegments().Count;
            if (Settings.Default.Application_DefaultVehicleSegmentPrice != null)
            {
                var SelectedSegment = _parkingService.GetVehicleSegmentById(Settings.Default.Application_DefaultVehicleSegmentPrice);
                if (SelectedSegment != null)
                {
                    ViewModel.SelectedSegment = SelectedSegment;
                }
            }
            //else if (_parkingService.GetVehicleSegments().Count > 0)
            //{
            //    VehicleSegmentsComboBox.SelectedIndex = 0;
            //}

            ViewModel.IsActive = true;
            InitializeCardReader();
            #region لود کردن لیست حروف پلاک
            var plateChars = LicensePlateHelper.GetChars();
            plateCharsCombo.ItemsSource = plateChars.Select(p => p.PlateFa).ToList();
            #endregion
            this.Unloaded += Page_Unloaded;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (nfc != null)
            {
                nfc.CardUidReceived -= OnCardUidReceivedSlot;
                nfc = null;
            }
        }
        private NFC nfc = new NFC();
        private void InitializeCardReader()
        {
            if (Settings.Default.Application_EntryCardRequirement)
            {
                try
                {

                    nfc.Init(0, false);
                    nfc.CardUidReceived += OnCardUidReceivedSlot;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    return;
                }
            }
        }
        private void OnCardUidReceivedSlot(byte[] uid)
        {
            try
            {
                ViewModel.CardSerialNo = uid.GetCardUID();
                UpsertCard();
                ViewModel.CardSerialNo = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
        private bool UpsertCard()
        {
            bool check = CheckValidation();
            if (ViewModel.CardSerialNo != null && check)
            {
                if (_parkingService.IsCardInUse((long)ViewModel.CardSerialNo))
                {
                    ShowMessage("خطا", "کارت پر میباشت، لطفا در گیت خروجی نسبت به خالی کردن کارت اقدام فرمایید");
                    return false;
                }
                CardModel card = new CardModel()
                {
                    ActiveDate = DateTime.Now,
                    CardSerialNo = ViewModel.CardSerialNo,
                    DeactiveDate = DateTime.Now.AddDays(ViewModel.ValidityPeriod),
                    IsActive = ViewModel.IsActive,
                    FixDiscount = ViewModel.FixDiscount,
                    PercentDiscount = ViewModel.PercentDiscount,
                    OwnerFirstName = ViewModel.OwnerFirstName,
                    OwnerLastName = ViewModel.OwnerLastName,
                    VehicleSegmentId = ViewModel.SelectedSegment.Id,
                    EnLicensePlate = ViewModel.EnLicensePlate, 
                    OwnerNationalCode = ViewModel.OwnerNationalCode, 
                    OwnerPhoneNumber = ViewModel.OwnerPhoneNumber, 
                };
                var result = _parkingService.AddCard(card);
                if (result)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ShowCardInfoBox(card.CardSerialNo.ToString());
                    });

                    return true;
                }
                return false;
            }
            return false;
        }
        private void ShowCardInfoBox(string CardNo)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    UidTextBlock.Text = CardNo;
                    SaveCardInfoBox.Visibility = Visibility.Visible;
                });
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.Dispatcher.Invoke(() =>
                    {
                        UidTextBlock.Text = CardNo;
                        SaveCardInfoBox.Visibility = Visibility.Collapsed;
                    });
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        private bool CheckValidation()
        {
            try
            {
                return this.Dispatcher.Invoke(() =>
                {
                    if (VehicleSegmentsComboBox.SelectedItem == null)
                    {
                        ShowMessage("خطا", "لطفا تعرفه مربوط به کارت را وارد کنید");
                        return false;
                    }
                    if (!PercentDiscountTextBox.Text.IsNumeric())
                    {
                        ShowMessage("خطا", "لطفا درصد تخفیف را بدرستی وارد کنید");
                        return false;
                    }
                    if (int.Parse(PercentDiscountTextBox.Text) > 100)
                    {
                        ShowMessage("خطا", "لطفا درصد تخفیف را بدرستی وارد کنید");
                        return false;
                    }
                    if (!ValidityPeriodTextBox.Text.IsNumeric())
                    {
                        ShowMessage("خطا", "لطفا مدت اعتبار را بدرستی وارد کنید");
                        return false;
                    }
                    if (int.Parse(ValidityPeriodTextBox.Text) < 1)
                    {
                        ShowMessage("خطا", "لطفا مدت اعتبار را بدرستی وارد کنید");
                        return false;
                    }
                    if ((bool)PlateAssignmentToggle.IsChecked)
                    {
                        CheckPlate();
                        if (!CheckPlate())
                        {
                            ShowMessage("خطا", "لطفا شماره پلاک را بدرستی وارد کنید");
                            return false;
                        }
                        var plateChar = plateCharsCombo.SelectedValue.ToString();
                        ViewModel.EnLicensePlate = $"{leftNumbersNumberTextBox.Text}_{plateChar.ConvertFaCharToEnCharIndex().ToLower()}_{rightNumbersNumberTextBox.Text}_IR{irNumberTextBox.Text}";
                    }
                    return true;
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }

        }
        private bool CheckPlate()
        {
            try
            {
                return this.Dispatcher.Invoke(() =>
                {
                    if (leftNumbersNumberTextBox.Text.IsNumeric() && rightNumbersNumberTextBox.Text.IsNumeric())
                    {
                        if (leftNumbersNumberTextBox.Text.Length == 2 && irNumberTextBox.Text.Length == 2 && rightNumbersNumberTextBox.Text.Length == 3)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
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

        private void PlateAssignmentToggle_Checked(object sender, RoutedEventArgs e)
        {
            PlateBox.Visibility = Visibility.Visible;
        }

        private void PlateAssignmentToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            PlateBox.Visibility = Visibility.Collapsed;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.ValidityPeriod = ValidityPeriodTextBox.Text.IsNumeric() ? int.Parse(ValidityPeriodTextBox.Text) : 0;
        }
    }
}
