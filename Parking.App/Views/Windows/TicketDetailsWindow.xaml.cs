using Parking.App.Attributes;
using System.Printing;
using System.Windows.Markup;
using System.Windows.Xps;
using Border = Wpf.Ui.Controls.Border;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Grid = Wpf.Ui.Controls.Grid;
using Image = Wpf.Ui.Controls.Image;
using Size = System.Windows.Size;
using StackPanel = Wpf.Ui.Controls.StackPanel;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace Parking.App.Views.Windows
{
    /// <summary>
    /// Interaction logic for TicketDetailsWindow.xaml
    /// </summary>
    public partial class TicketDetailsWindow : FluentWindow
    {
        public List<CameraConfigModel> Cameras { get; set; }
        public TicketDetailsWindowViewModel ViewModel { get; set; } = new TicketDetailsWindowViewModel();
        public HotKeyManagementViewModel _hotKeyVm { get; set; } = new HotKeyManagementViewModel();
        private readonly IParkingService? _parkingService;
        private bool IsMissingCard { get; set; } = false;
        private bool PaymentPermission { get; set; } = true;
        private string ExitImage { get; set; }
        private readonly ILogger<TicketDetailsWindow> _logger;
        private List<(ImageSource ImageSource, string Name, bool ForSave)> ExtraImagesList = new List<(ImageSource ImageSource, string Name, bool ForSave)>();
        private DispatcherTimer _closeTimer;
        public TicketDetailsWindow(Guid ticketId, BitmapSource currentImage, List<(string Image, string Name)>? extraimages, long? cardUid)
        {
            try
            {
                _parkingService = App.GetService<IParkingService>();
                _logger = App.GetService<ILogger<TicketDetailsWindow>>();
                Cameras = CameraConfigManager.GetActiveCameras();

                if (ticketId != null)
                {
                    this.DataContext = ViewModel;
                    ViewModel.TicketId = ticketId;
                    InitializeComponent();
                    if (Cameras.Any())
                    {
                        ExtraColumn.Width = new GridLength(1, GridUnitType.Star);
                        ExtraImages.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ExtraColumn.Width = new GridLength(0);
                        ExtraImages.Visibility = Visibility.Collapsed;
                    }
                    if (currentImage != null)
                    {
                        currentImg.Source = currentImage;
                        ExitImage = currentImage.ResizeAndCompressBitmap(1024, 768, 72, 72, 65);
                    }
                    SetTicketData(ticketId);

                    if (extraimages != null)
                    {
                        foreach (var image in extraimages)
                        {
                            if (image.Image != null)
                            {
                                ExtraImagesList.Add((ImageHelper.Base64ToImageSource(image.Image), image.Name, true));
                            }
                        }
                    }
                    if (cardUid != null)
                    {
                        if (cardUid == 0 && !PermissionHelper.CheckUserPermission("ForceExitRequest"))
                        {
                            PaymentPermission = false;
                        }
                    }
                }
                else
                {
                    ShowMessage("خطا", "خطا در نمایش، لطفا دوباره تلاش کنید");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("خطا", "خطا در نمایش، لطفا دوباره تلاش کنید");
                _logger.LogError("Error in Ticket Details", ex);
            }
            UpdateButtonsTitle();
        }
        private void UpdateButtonsTitle()
        {
            PaymentBtn.Content = $"پرداخت ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.PaymentWithSpace))})";
            CashPaymentBtn.Content = $"پرداخت نقدی ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.CashPayment))})";
            PrintBtn.Content = $"چاپ رسید ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.PrintReceipt))})";
            MissingCardToggleLabel.Content = $"آیا کارت مفقود شده است؟ (کلید {_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.LostCard))} برای فعال شدن)";
        }
        private void InitializeCloseTimer()
        {
            if (Settings.Default.Appearance_InvoiceShowTime > 0)
            {
                _closeTimer = new DispatcherTimer();
                _closeTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Appearance_InvoiceShowTime);
                _closeTimer.Tick += CloseTimer_Tick;
                _closeTimer.Start();
            }
        }
        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            this.Close();
        }

        private async Task SetTicketData(Guid ticketId)
        {
            try
            {
                var ticket = await _parkingService?.GetTicketDetailsAsync(ticketId);

                if (ticket != null)
                {
                    ticket.PaidType = ticket.PaidType?.ToLower().Replace("naghdi", "نقدی").Replace("POS", "دستگاه کارتخوان");

                    if (!(bool)ticket.IsExited)
                    {
                        if (ticket.StartTime.Date == DateTime.Now.Date)
                        {
                            ticket.StartTimeString = "امروز";
                        }
                        if (ticket.EndTime?.Date == DateTime.Now.Date)
                        {
                            ticket.EndTimeString = "امروز";
                        }
                        ticket.EndTimeString = string.Empty;
                        ticket.EndTimeOnlyString = string.Empty;
                    }

                    ViewModel.Item = ticket;
                    SetPlate(ViewModel.Item?.EnLicensePlate ?? "--_-_---_IR--");
                    ViewModel.Title = ticket.LicensePlate;

                    var entryimage = await _parkingService.GetTicketImages(ticketId);
                    await this.Dispatcher.InvokeAsync(() => EntryImage.Source = entryimage.StartImage);
                    if (entryimage.ExitImage != null)
                    {
                        await this.Dispatcher.InvokeAsync(() => currentImg.Source = entryimage.ExitImage);
                    }
                    LoadImages(ticketId);
                    CheckSeizedPlate();
                    this.Topmost = true;
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    //DataContext = ViewModel;
                    InitializeCloseTimer();

                    if (ViewModel.Item?.IsPaid ?? false)
                    {
                        SetPaymentStatus(true);
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CashPaymentBtn.Visibility = Visibility.Collapsed;
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CustomPayment_Btn.Visibility = Visibility.Collapsed;
                        //MissingCardToggle.IsChecked = true;
                        MissingCardToggle.IsEnabled = false;
                    }
                    // ورودی=0   خروجی=1
                    if (!Settings.Default.Application_GateType.ToString().Contains("1"))
                    {
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CashPaymentBtn.Visibility = Visibility.Collapsed;
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CustomPayment_Btn.Visibility = Visibility.Collapsed;
                        MissingCardToggle.IsEnabled = false;
                    }
                    if (!PaymentPermission)
                    {
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CashPaymentBtn.Visibility = Visibility.Collapsed;
                        PaymentBtn.Visibility = Visibility.Collapsed;
                        CustomPayment_Btn.Visibility = Visibility.Collapsed;
                        MissingCardToggle.IsEnabled = false;
                    }
                    MainPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                    ms.Title = "خطا";
                    ms.Content = "خطا در دریافت اطلاعات";
                    ms.IsPrimaryButtonEnabled = false;
                    ms.IsSecondaryButtonEnabled = false;
                    ms.CloseButtonText = "متوجه شدم";
                    await ms.ShowDialogAsync();
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SetTicketData in Ticket Details", ex);
            }

        }
        private void CheckSeizedPlate()
        {
            if (_parkingService?.IsSeizedLicensePlate(ViewModel.Item.EnLicensePlate ?? "_") == true)
            {
                //TitleBar.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
                seizedBox.Visibility = Visibility.Visible;
            }
            exitedStatus.IsExited = ViewModel.Item.IsExited ?? false;
        }
        private void SetPlate(string enLicensePlate)
        {
            var plate = enLicensePlate.ParsePlate();
            if (plate.IsIranianPlate)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.OtherPlateBox.Visibility = Visibility.Collapsed;
                    this.IRPlateBox.Visibility = Visibility.Visible;

                });
                plate_LeftNumber.Text = plate.LeftTwoDigits;
                plate_RightNumber.Text = plate.RightThreeDigits;
                plate_IRNumber.Text = plate.IranCode.Replace("IR", "");
                plate_Char.Text = plate.Letter.ConvertEnCharToFaCharIndex();

            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.IRPlateBox.Visibility = Visibility.Collapsed;
                    this.OtherPlateBox.Visibility = Visibility.Visible;
                    this.OtherPlateTextBox.Text = enLicensePlate;
                });
            }


            PlateCharName.Content = plate.Letter.ConvertToString();

        }
        private void TicketDetailsWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                return;
            }

            ModifierKeys currentModifiers = Keyboard.Modifiers;

            bool IsHotKeyPressed(HotKeyConfig? config)
            {
                return config != null &&
                       config.Key == (e.Key == Key.System ? e.SystemKey : e.Key) &&
                       config.Modifiers == currentModifiers;
            }

            if (Settings.Default.Application_GateType.Contains("1"))
            {
                if (PermissionHelper.CheckUserPermission("CashPayment"))
                {
                    var cashPaymentHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.CashPayment);
                    if (IsHotKeyPressed(cashPaymentHotKey))
                    {
                        if (!ViewModel.Item?.IsPaid ?? false)
                        {
                            CashPayment();
                        }
                        else
                        {
                            ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                        }
                    }
                }

                if (PermissionHelper.CheckUserPermission("PosPayment"))
                {
                    var paymentWithSpaceHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.PaymentWithSpace);
                    if (IsHotKeyPressed(paymentWithSpaceHotKey))
                    {
                        if (!ViewModel.Item?.IsPaid ?? false)
                        {
                            Payment();
                        }
                        else
                        {
                            ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                        }
                    }
                }



                var missingCardHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.LostCard);
                if (IsHotKeyPressed(missingCardHotKey))
                {
                    if (!ViewModel.Item?.IsPaid ?? false)
                    {
                        MissingCardToggle.IsChecked = !IsMissingCard;
                    }
                    else
                    {
                        ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                    }
                }

                var printReceiptHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.PrintReceipt);
                if (IsHotKeyPressed(printReceiptHotKey))
                {
                    if (PermissionHelper.CheckUserPermission("PrintTicket"))
                    {
                        PrintTicket();
                    }
                }
            }
        }
        public async void Payment()
        {
            if (!PermissionHelper.CheckUserPermission("PosPayment"))
                return;

            try
            {

                if (ViewModel.Item.IsPaid == false)
                {
                    if (PaymentPermission)
                    {
                        if (Settings.Default.Application_GatePCName.Length > 3)
                        {
                            GateName = Settings.Default.Application_GatePCName;
                        }
                        else
                        {
                            GateName = System.Environment.MachineName;
                        }
                        OmidPayPcPos.OmidPayPcPosClass pos = new OmidPayPcPos.OmidPayPcPosClass();
                        if (ViewModel.Item.TotalAmount > 1000)
                        {
                            string amount = ViewModel.Item.TotalAmount.RoundAndRemoveDecimals().ToString();

                            var result = pos.DoTcpTransaction(Settings.Default.POS_IP, Settings.Default.POS_Port, amount, null, null, OmidPayPcPos.OmidPayPcPosClass.POSAPPTYPE.OMD);
                            if (result.Result == "OK")
                            {
                                var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                                {
                                    PaidAmount = decimal.Parse(result.SpentAmount),
                                    PaidCreditCard = result.CardNo,
                                    PaidType = "POS",
                                    RefId = result.RRN,
                                    TicketId = ViewModel.Item.Id,
                                    MerchantNumber = ViewModel.Item.MerchantNumber,
                                    PaidDate = ViewModel.Item.PaidDate,
                                    RRN = ViewModel.Item.RRN,
                                    TraceNo = result.TraceNo,
                                    ExitGate = GateName,
                                    IsMissingCard = IsMissingCard,
                                    CardUid = ViewModel.Item.CardUid,
                                    ExitRegistrarUserId = TokenStore.UserId,
                                    ExitImage = ExitImage
                                });
                                SetTicketData(ViewModel.Item.Id);
                                SetPaymentStatus(true);
                                CloseAfterSuccessPayment();
                            }
                            else
                            {
                                SetPaymentStatus(false);
                            }
                        }
                        else
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
                                PaidAmount = 0,
                                PaidCreditCard = "",
                                PaidType = "Naghdi",
                                RefId = "0000",
                                TicketId = ViewModel.Item.Id,
                                MerchantNumber = "00",
                                PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                                RRN = "000",
                                TraceNo = "00000",
                                ExitGate = GateName,
                                IsMissingCard = IsMissingCard,
                                CardUid = ViewModel.Item.CardUid,
                                ExitImage = ExitImage
                            });
                            if (rs)
                            {

                                SetTicketData(ViewModel.Item.Id);
                                SetPaymentStatus(true);
                                SaveExtraImages();
                                CloseAfterSuccessPayment();
                            }
                            else
                            {
                                SetPaymentStatus(false);
                            }

                        }
                    }

                }
                else
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

            }
            catch
            {
                ShowMessage("خطا", "خطا در پرداخت با دستگاه کارتخوان");
                return;
            }
        }

        private void CashPayment_Click(object sender, RoutedEventArgs e) => CashPayment();
        private void MissingCardToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsMissingCard)
            {
                IsMissingCard = true;
                ViewModel.Item.TotalAmount += Settings.Default.Application_MissingCardPrice;
                ViewModel.Item = ViewModel.Item;
            }
        }

        private void MissingCardToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsMissingCard)
            {
                IsMissingCard = false;
                ViewModel.Item.TotalAmount -= Settings.Default.Application_MissingCardPrice;
                ViewModel.Item = ViewModel.Item;
            }
        }
        private void SetPaymentStatus(bool result)
        {
            if (result)
            {
                paymentResultBox.Visibility = Visibility.Visible;
                //TitleBar.Background = new SolidColorBrush(System.Windows.Media.Colors.Green);
                paymentResultBox.Background = new SolidColorBrush(System.Windows.Media.Colors.Green);
                paymentResultText.Text = "پرداخت موفق";

                PaymentBtn.Visibility = Visibility.Collapsed;
                CashPaymentBtn.Visibility = Visibility.Collapsed;
                PaymentBtn.Visibility = Visibility.Collapsed;
                MissingCardToggle.IsEnabled = false;
            }
            else
            {
                paymentResultBox.Visibility = Visibility.Visible;
                //TitleBar.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
                paymentResultBox.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
                paymentResultText.Text = "پرداخت ناموفق";
            }
        }
        private void ShowMessage(string title, string message)
        {
            try
            {
                if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
                {
                    Application.Current.Dispatcher.Invoke(async () =>
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
        private async void LoadImages(Guid ticketId)
        {
            try
            {


                var extraImages = await _parkingService.GetTicketExtraImageSourcesAsync(ticketId, true);
                if (extraImages != null)
                {
                    foreach (var image in extraImages)
                    {
                        if (image.ImageSource != null)
                        {
                            ExtraImagesList.Add(new(image.ImageSource, image.FaName ?? "_", false));
                        }
                    }
                }

                await this.Dispatcher.InvokeAsync(async () =>
                {
                    AddImageListToExtraImageBox(ExtraImagesList);
                });

            }
            catch
            {
                //EntryImage.Source = null;
            }

        }

        [RequiresPermission("PosPayment", "پرداخت با پوز")]
        private void PaymentBtn_Click(object sender, RoutedEventArgs e)
        {
            Payment();
        }
        private string GateName { get; set; }

        [RequiresPermission("CashPayment", "پرداخت نقدی")]
        public async void CashPayment()
        {
            if (!PermissionHelper.CheckUserPermission("CashPayment"))
                return;
            try
            {
                if (ViewModel.Item.IsPaid == false)
                {
                    if (PaymentPermission)
                    {
                        if (Settings.Default.Application_GatePCName.Length > 3)
                            GateName = Settings.Default.Application_GatePCName;
                        else
                            GateName = Environment.MachineName;


                        if (ViewModel.Item.TotalAmount > 1000)
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
                                PaidAmount = ViewModel.Item.TotalAmount,
                                TotalAmount = ViewModel.Item.TotalAmount,
                                PaidCreditCard = "",
                                PaidType = "Naghdi",
                                RefId = "0000",
                                TicketId = ViewModel.Item.Id,
                                MerchantNumber = "00",
                                PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                                RRN = "000",
                                TraceNo = "00000",
                                ExitGate = GateName,
                                IsMissingCard = IsMissingCard,
                                CardUid = ViewModel.Item.CardUid,
                                ExitRegistrarUserId = TokenStore.UserId,
                                ExitImage = ExitImage,
                            });

                        }
                        else
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
                                TotalAmount = ViewModel.Item.TotalAmount,
                                PaidAmount = ViewModel.Item.TotalAmount,
                                PaidCreditCard = "",
                                PaidType = "Naghdi",
                                RefId = "0000",
                                TicketId = ViewModel.Item.Id,
                                MerchantNumber = "00",
                                PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                                RRN = "000",
                                TraceNo = "00000",
                                ExitGate = GateName,
                                IsMissingCard = IsMissingCard,
                                CardUid = ViewModel.Item.CardUid,
                                ExitImage = ExitImage
                            });
                        }
                        SaveExtraImages();
                        SetTicketData(ViewModel.Item.Id);
                        SetPaymentStatus(true);
                    }
                }
                else
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

            }
            catch
            {
                ShowMessage("خطا", "خطا در پرداخت با دستگاه کارتخوان");
                return;
            }
        }

        public async void CustomPayment()
        {
            if (!PermissionHelper.CheckUserPermission("CustomAmouontPayment"))
                return;
            try
            {
                if (ViewModel.Item.IsPaid == false)
                {
                    if (PaymentPermission)
                    {
                        if (Settings.Default.Application_GatePCName.Length > 3)
                            GateName = Settings.Default.Application_GatePCName;
                        else
                            GateName = Environment.MachineName;


                        if (ViewModel.Item.TotalAmount > 1000)
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
                                PaidAmount = ViewModel.Item.PaidAmount,
                                TotalAmount = ViewModel.Item.TotalAmount,
                                PaidCreditCard = "",
                                PaidType = "Naghdi",
                                RefId = "0000",
                                TicketId = ViewModel.Item.Id,
                                MerchantNumber = "00",
                                PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                                RRN = "000",
                                TraceNo = "00000",
                                ExitGate = GateName,
                                IsMissingCard = IsMissingCard,
                                CardUid = ViewModel.Item.CardUid,
                                ExitRegistrarUserId = TokenStore.UserId,
                                ExitImage = ExitImage,
                                IsCustomPaid = true
                            });

                        }
                        else
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
                                TotalAmount = ViewModel.Item.TotalAmount,
                                PaidAmount = ViewModel.Item.PaidAmount,
                                PaidCreditCard = "",
                                PaidType = "Naghdi",
                                RefId = "0000",
                                TicketId = ViewModel.Item.Id,
                                MerchantNumber = "00",
                                PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                                RRN = "000",
                                TraceNo = "00000",
                                ExitGate = GateName,
                                IsMissingCard = IsMissingCard,
                                CardUid = ViewModel.Item.CardUid,
                                ExitImage = ExitImage,
                                IsCustomPaid = true
                            });

                        }
                        SaveExtraImages();
                        SetTicketData(ViewModel.Item.Id);
                        SetPaymentStatus(true);
                    }

                }
                else
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

            }
            catch
            {
                ShowMessage("خطا", "خطا در پرداخت با دستگاه کارتخوان");
                return;
            }
        }


        private void AddImageListToExtraImageBox(List<(ImageSource imageSource, string title, bool ForSave)> list)
        {

            try
            {

                foreach (var image in list)
                {
                    Border border = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(16, 206, 206, 206)),
                        Background = Brushes.Transparent,
                        Margin = new Thickness()
                        {
                            Bottom = 0,
                            Left = 5,
                            Right = 5,
                            Top = 5
                        }
                    };


                    Grid grid = new Grid();

                    Image imageControl = new Image
                    {
                        Stretch = Stretch.Fill,
                        Source = image.imageSource,
                        Height = 200
                    };

                    StackPanel overlayPanel = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
                    };

                    TextBlock textBlock = new TextBlock
                    {
                        Text = image.title,
                        Foreground = Brushes.White,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(5)

                    };

                    overlayPanel.Children.Add(textBlock);
                    grid.Children.Add(imageControl);
                    grid.Children.Add(overlayPanel);
                    border.Child = grid;

                    ExtraImages.Children.Add(border);
                }


            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }


        }

        private void SaveExtraImages()
        {
            foreach (var image in ExtraImagesList.Where(e => e.ForSave))
            {
                _parkingService?.AddTicketExtraImage(ViewModel.Item.Id, image.ImageSource.ImageSourceToBase64(), image.Name, true);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        [RequiresPermission("PrintTicket", "چاپ قبض")]
        private async void Print_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("PrintTicket"))
                return;
            PrintTicket();
        }

        [RequiresPermission("CustomAmouontPayment", "خروج با مبلغ دلخواه")]
        private void CustomPayment_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("CustomAmouontPayment"))
                return;
            CustomAmountPaymentModalWindow customAmountPaymentModalWindow = new CustomAmountPaymentModalWindow(ViewModel.TicketId)
            {
                Owner = this
            };
            var result = customAmountPaymentModalWindow.ShowDialog();

            if (result == true)
            {
                ViewModel.Item.PaidAmount = customAmountPaymentModalWindow.ViewModel.Amount;
                CustomPayment();
            }
        }
        private async void PrintTicket()
        {
            try
            {
                var ticket = await _parkingService?.GetTicketDetailsAsync(ViewModel.Item.Id);


                double dpi = Settings.Default.Application_Print_dpi;
                double widthMm = Settings.Default.Application_Print_widthMm;
                double widthInches = widthMm / 25.4;
                double widthPixels = dpi * widthInches;


                var receiptContent = ReceiptPrinter.GenerateInvoiceContent(new InvoiceModel
                {
                    BarcodeId = ticket.BarcodeId,
                    DriverDescription = ticket.DriverDescription,
                    QueueNumber = ticket.QueueNumber,
                    Description = ticket.Description,
                    LicensePlate = ticket.LicensePlate,
                    ParkingName = ticket.ParkingName,
                    StartTime = ticket.StartTime.ToShamsi() + "  " + ticket.StartTime.ToString("HH:mm"),
                    VehicleSegmentName = ticket.VehicleManufacturerName,
                    EndTime = ((ticket.IsExited ?? false) && ticket.EndTime != null) ? ticket.EndTime?.ToShamsi() + " " + ticket.EndTime?.ToString("HH:mm") : "",
                    PaidAmount = ticket.PaidAmount.ToString("N0"),
                    TotalAmount = ticket.TotalAmount.ToString("N0"),
                    TotalDiscount = ticket.Discount.ToString("N0")
                }, widthPixels);
                // Print the receipt 
                // PrintHelper.Print(receiptContent);

                DirectPrint(receiptContent);
            }
            catch
            {

                ShowMessage("خطا در پرینت", "خطا");
                return;
            }
        }

        private void DirectPrint(UIElement contentToPrint)
        {
            // تنظیمات پرینتر
            PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();
            PrintTicket printTicket = printQueue.DefaultPrintTicket;


            double dpi = Settings.Default.Application_Print_dpi;
            double widthMm = Settings.Default.Application_Print_widthMm;
            double widthInches = widthMm / 25.4;
            double widthPixels = dpi * widthInches;


            contentToPrint.Measure(new Size(widthPixels, double.PositiveInfinity));
            contentToPrint.Arrange(new Rect(new System.Windows.Point(0, 0), contentToPrint.DesiredSize));
            double contentHeight = contentToPrint.DesiredSize.Height;

            // تنظیم اندازه صفحه بر اساس محتوای واقعی
            Size pageSize = new Size(widthPixels, contentHeight);

            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.DocumentPaginator.PageSize = pageSize;

            FixedPage fixedPage = new FixedPage
            {
                Width = pageSize.Width,
                Height = pageSize.Height
            };

            FixedPage.SetLeft(contentToPrint, 0);
            FixedPage.SetTop(contentToPrint, 0);
            fixedPage.Children.Add(contentToPrint);

            PageContent pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            // ارسال به پرینتر
            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
            writer.Write(fixedDoc, printTicket);
        }

        [RequiresPermission("ForceExitRequest", "درخواست خروج اجباری")]

        private async void CloseAfterSuccessPayment()
        {
            bool active = Settings.Default.Application_CloseTicketAfterSuccessPayment;
            if (active)
            {
                await Task.Delay(2000);
                this.Close();
            }
        }
    }
}
