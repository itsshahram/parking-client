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
    public partial class TicketDetailsWindow : FluentWindow
    {
        public List<CameraConfigModel> Cameras { get; set; }
        public TicketDetailsWindowViewModel ViewModel { get; set; } = new TicketDetailsWindowViewModel();
        public HotKeyManagementViewModel _hotKeyVm { get; set; } = new HotKeyManagementViewModel();

        private readonly IParkingService? _parkingService;
        private readonly ILogger<TicketDetailsWindow> _logger;

        private bool IsMissingCard { get; set; } = false;
        private bool PaymentPermission { get; set; } = true;
        private string ExitImage { get; set; } = string.Empty;
        private string GateName { get; set; } = string.Empty;

        private readonly List<(ImageSource ImageSource, string Name, bool ForSave)> ExtraImagesList = new();
        private DispatcherTimer _closeTimer;

        private bool _ticketLoaded;

        private readonly SemaphoreSlim _paymentGate = new(1, 1);

        public TicketDetailsWindow(Guid ticketId, BitmapSource currentImage, List<(string Image, string Name)>? extraimages, long? cardUid)
        {
            try
            {
                _parkingService = App.GetService<IParkingService>();
                _logger = App.GetService<ILogger<TicketDetailsWindow>>();
                Cameras = CameraConfigManager.GetActiveCameras();

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

                _ = SetTicketData(ticketId);

                if (extraimages != null)
                {
                    foreach (var image in extraimages)
                    {
                        if (image.Image != null)
                            ExtraImagesList.Add((ImageHelper.Base64ToImageSource(image.Image), image.Name, true));
                    }
                }

                if (cardUid != null)
                {
                    if (cardUid == 0 && !PermissionHelper.CheckUserPermission("ForceExitRequest"))
                        PaymentPermission = false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage("خطا", "خطا در نمایش، لطفا دوباره تلاش کنید");
                _logger?.LogError(ex, "Error in Ticket Details");
                Close();
            }

            UpdateButtonsTitle();
        }

        private void UpdateButtonsTitle()
        {
            PaymentBtn.Content = $"پرداخت ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.PaymentWithSpace))})";
            CashPaymentBtn.Content = $"پرداخت نقدی ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.CashPayment))})";
            PrintBtn.Content = $"چاپ رسید ({_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.PrintReceipt))})";
            MissingCardToggleLabel.Content =
                $"آیا کارت مفقود شده است؟ (کلید {_hotKeyVm.FormatHotkey(_hotKeyVm.GetHotKey(HotKeyActionType.LostCard))} برای فعال شدن)";
        }

        private void InitializeCloseTimer()
        {
            if (Settings.Default.Appearance_InvoiceShowTime > 0)
            {
                _closeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(Settings.Default.Appearance_InvoiceShowTime)
                };
                _closeTimer.Tick += CloseTimer_Tick;
                _closeTimer.Start();
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            Close();
        }


        private async Task SetTicketData(Guid ticketId)
        {
            _ticketLoaded = false;

            try
            {

                LoadingProgress.Visibility = Visibility.Visible;
                MainPanel.Visibility = Visibility.Collapsed;

                var ticket = await _parkingService?.GetTicketDetailsAsync(ticketId);

                if (ticket == null)
                {
                    var ms = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "خطا",
                        Content = "خطا در دریافت اطلاعات",
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false,
                        CloseButtonText = "متوجه شدم"
                    };
                    await ms.ShowDialogAsync();
                    return;
                }

                ticket.PaidType = ticket.PaidType?
                    .ToLower()
                    .Replace("naghdi", "نقدی")
                    .Replace("POS", "دستگاه کارتخوان");

                if (ticket.IsExited != true)
                {
                    if (ticket.StartTime.Date == DateTime.Now.Date)
                        ticket.StartTimeString = "امروز";

                    if (ticket.EndTime?.Date == DateTime.Now.Date)
                        ticket.EndTimeString = "امروز";

                    ticket.EndTimeString = string.Empty;
                    ticket.EndTimeOnlyString = string.Empty;
                }

                ViewModel.Item = ticket;

                SetPlate(ticket.EnLicensePlate ?? "--_-_---_IR--");
                ViewModel.Title = ticket.LicensePlate;

                // --- Images
                var entryimage = await _parkingService.GetTicketImages(ticketId);

                await Dispatcher.InvokeAsync(() =>
                {
                    EntryImage.Source = entryimage.StartImage;
                    if (entryimage.ExitImage != null)
                        currentImg.Source = entryimage.ExitImage;
                });

                LoadImages(ticketId);

                CheckSeizedPlate();

                Topmost = true;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                InitializeCloseTimer();

                if (ticket.IsPaid == true)
                {
                    SetPaymentStatus(true);
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    CashPaymentBtn.Visibility = Visibility.Collapsed;
                    CustomPayment_Btn.Visibility = Visibility.Collapsed;
                    MissingCardToggle.IsEnabled = false;
                }

                // ورودی=0   خروجی=1
                if (!Settings.Default.Application_GateType.ToString().Contains("1"))
                {
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    CashPaymentBtn.Visibility = Visibility.Collapsed;
                    CustomPayment_Btn.Visibility = Visibility.Collapsed;
                    MissingCardToggle.IsEnabled = false;
                }

                if (!PaymentPermission)
                {
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    CashPaymentBtn.Visibility = Visibility.Collapsed;
                    CustomPayment_Btn.Visibility = Visibility.Collapsed;
                    MissingCardToggle.IsEnabled = false;
                }

                _ticketLoaded = true;
                UpdateExitedStatus();
                LoadingProgress.Visibility=Visibility.Collapsed;
                MainPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                _ticketLoaded = true;
                _logger?.LogError(ex, "Error in SetTicketData in Ticket Details");
            }
        }

        private void UpdateExitedStatus()
        {
            if (!_ticketLoaded || ViewModel.Item == null)
                return;

            exitedStatus.IsExited = ViewModel.Item.IsExited ?? false;
        }


        private void CheckSeizedPlate()
        {
            if (_parkingService?.IsSeizedLicensePlate(ViewModel.Item.EnLicensePlate ?? "_") == true)
                seizedBox.Visibility = Visibility.Visible;

            exitedStatus.IsExited = ViewModel.Item.IsExited ?? false;
        }

        private void SetPlate(string enLicensePlate)
        {
            var plate = enLicensePlate.ParsePlate();

            if (plate.IsIranianPlate)
            {
                Dispatcher.Invoke(() =>
                {
                    OtherPlateBox.Visibility = Visibility.Collapsed;
                    IRPlateBox.Visibility = Visibility.Visible;
                });

                plate_LeftNumber.Text = plate.LeftTwoDigits;
                plate_RightNumber.Text = plate.RightThreeDigits;
                plate_IRNumber.Text = plate.IranCode.Replace("IR", "");
                plate_Char.Text = plate.Letter.ConvertEnCharToFaCharIndex();
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    IRPlateBox.Visibility = Visibility.Collapsed;
                    OtherPlateBox.Visibility = Visibility.Visible;
                    OtherPlateTextBox.Text = enLicensePlate;
                });
            }

            PlateCharName.Content = plate.Letter.ConvertToString();
        }

        private async void TicketDetailsWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            var currentModifiers = Keyboard.Modifiers;

            bool IsHotKeyPressed(HotKeyConfig? config) =>
                config != null &&
                config.Key == (e.Key == Key.System ? e.SystemKey : e.Key) &&
                config.Modifiers == currentModifiers;

            if (!Settings.Default.Application_GateType.Contains("1"))
                return;

            if (PermissionHelper.CheckUserPermission("CashPayment"))
            {
                var cashPaymentHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.CashPayment);
                if (IsHotKeyPressed(cashPaymentHotKey))
                {
                    if (!ViewModel.Item?.IsPaid ?? false)
                        await CashPayment();
                    else
                        ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                }
            }

            if (PermissionHelper.CheckUserPermission("PosPayment"))
            {
                var paymentWithSpaceHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.PaymentWithSpace);
                if (IsHotKeyPressed(paymentWithSpaceHotKey))
                {
                    if (!ViewModel.Item?.IsPaid ?? false)
                        await Payment();
                    else
                        ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                }
            }

            var missingCardHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.LostCard);
            if (IsHotKeyPressed(missingCardHotKey))
            {
                if (!ViewModel.Item?.IsPaid ?? false)
                    MissingCardToggle.IsChecked = !IsMissingCard;
                else
                    ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
            }

            var printReceiptHotKey = _hotKeyVm.GetHotKey(HotKeyActionType.PrintReceipt);
            if (IsHotKeyPressed(printReceiptHotKey))
            {
                if (PermissionHelper.CheckUserPermission("PrintTicket"))
                    PrintTicket();
            }
        }

        private void EnsureGateName()
        {
            GateName = Settings.Default.Application_GatePCName.Length > 3
                ? Settings.Default.Application_GatePCName
                : Environment.MachineName;
        }

        private (Guid ticketId, decimal totalAmount, decimal paidAmount, long? cardUid, bool isMissing, string exitImage, Guid? userId) SnapshotPaymentState()
        {
            return (
                ViewModel.Item.Id,
                ViewModel.Item.TotalAmount,
                ViewModel.Item.PaidAmount,
                ViewModel.Item.CardUid,
                IsMissingCard,
                ExitImage,
                TokenStore.UserId
            );
        }

        [RequiresPermission("PosPayment", "پرداخت با پوز")]
        public async Task Payment()
        {
            if (!PermissionHelper.CheckUserPermission("PosPayment"))
                return;

            ShowPaymentLoader();
            try
            {
                if (ViewModel.Item.IsPaid == true)
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

                if (!PaymentPermission)
                {
                    ShowMessage("خطا", "اجازه پرداخت وجود ندارد");
                    return;
                }

                EnsureGateName();

                var snap = SnapshotPaymentState();

                var pos = new OmidPayPcPos.OmidPayPcPosClass();

                if (snap.totalAmount <= 1000)
                {
                    bool rs = await _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel
                    {
                        PaidAmount = 0,
                        PaidCreditCard = "",
                        PaidType = "Naghdi",
                        RefId = "0000",
                        TicketId = snap.ticketId,
                        MerchantNumber = "00",
                        PaidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                        RRN = "000",
                        TraceNo = "00000",
                        ExitGate = GateName,
                        IsMissingCard = snap.isMissing,
                        CardUid = snap.cardUid,
                        ExitRegistrarUserId = snap.userId,
                        ExitImage = snap.exitImage
                    });

                    if (rs)
                    {
                        await SetTicketData(snap.ticketId);
                        SetPaymentStatus(true);
                        SaveExtraImages();
                        CloseAfterSuccessPayment();
                    }
                    else
                    {
                        SetPaymentStatus(false);
                    }

                    return;
                }

                string amount = snap.totalAmount.RoundAndRemoveDecimals().ToString();

                var posResult = await Task.Run(() =>
                    pos.DoTcpTransaction(
                        Settings.Default.POS_IP,
                        Settings.Default.POS_Port,
                        amount,
                        null,
                        null,
                        OmidPayPcPos.OmidPayPcPosClass.POSAPPTYPE.OMD
                    )
                );

                if (posResult == null || posResult.Result != "OK")
                {
                    SetPaymentStatus(false);
                    return;
                }

                var saveResult = await _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel
                {
                    PaidAmount = decimal.Parse(posResult.SpentAmount),
                    PaidCreditCard = posResult.CardNo,
                    PaidType = "POS",
                    RefId = posResult.RRN,
                    TicketId = snap.ticketId,
                    MerchantNumber = ViewModel.Item.MerchantNumber,
                    PaidDate = ViewModel.Item.PaidDate,
                    RRN = posResult.RRN,
                    TraceNo = posResult.TraceNo,
                    ExitGate = GateName,
                    IsMissingCard = snap.isMissing,
                    CardUid = snap.cardUid,
                    ExitRegistrarUserId = snap.userId,
                    ExitImage = snap.exitImage
                });

                if (!saveResult)
                {
                    SetPaymentStatus(false);
                    ShowMessage("خطا", "ذخیره پرداخت انجام نشد");
                    return;
                }

                await SetTicketData(snap.ticketId);
                SetPaymentStatus(true);
                SaveExtraImages();
                CloseAfterSuccessPayment();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in POS Payment");
                ShowMessage("خطا", "خطا در پرداخت با دستگاه کارتخوان");
            }
            finally
            {
                HidePaymentLoader();
            }
        }

        private async void CashPayment_Click(object sender, RoutedEventArgs e) => await CashPayment();

        [RequiresPermission("PosPayment", "پرداخت با پوز")]
        private async void PaymentBtn_Click(object sender, RoutedEventArgs e) => await Payment();

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
                paymentResultBox.Background = new SolidColorBrush(Colors.Green);
                paymentResultText.Text = "پرداخت موفق";

                PaymentBtn.Visibility = Visibility.Collapsed;
                CashPaymentBtn.Visibility = Visibility.Collapsed;
                MissingCardToggle.IsEnabled = false;
            }
            else
            {
                paymentResultBox.Visibility = Visibility.Visible;
                paymentResultBox.Background = new SolidColorBrush(Colors.Red);
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
                            ExtraImagesList.Add(new(image.ImageSource, image.FaName ?? "_", false));
                    }
                }

                await Dispatcher.InvokeAsync(() => AddImageListToExtraImageBox(ExtraImagesList));
            }
            catch
            {
                // ignore
            }
        }

        [RequiresPermission("CashPayment", "پرداخت نقدی")]
        public async Task CashPayment()
        {
            if (!PermissionHelper.CheckUserPermission("CashPayment"))
                return;

            if (!await _paymentGate.WaitAsync(0))
                return;

            try
            {
                if (ViewModel.Item?.IsPaid == true)
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

                if (!PaymentPermission)
                    return;

                EnsureGateName();

                var snap = SnapshotPaymentState();

                var model = new TicketPaidInfoModel
                {
                    TicketId = snap.ticketId,
                    TotalAmount = snap.totalAmount,
                    PaidAmount = snap.totalAmount,
                    PaidCreditCard = "",
                    PaidType = "Naghdi",
                    RefId = "0000",
                    MerchantNumber = "00",
                    PaidDate = DateTime.Now.ToString("yyyyMMdd"),
                    RRN = "000",
                    TraceNo = "00000",
                    ExitGate = GateName,
                    IsMissingCard = snap.isMissing,
                    CardUid = snap.cardUid,
                    ExitImage = snap.exitImage,
                    ExitRegistrarUserId = snap.totalAmount > 1000 ? snap.userId : null
                };

                await _parkingService.SetTicketPaidInfo(model);

                SaveExtraImages();
                await SetTicketData(snap.ticketId);
                SetPaymentStatus(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CashPayment failed");
                ShowMessage("خطا", "خطا در پرداخت نقدی");
            }
            finally
            {
                _paymentGate.Release();
            }
        }

        public async Task CustomPayment()
        {
            if (!PermissionHelper.CheckUserPermission("CustomAmouontPayment"))
                return;

            // Lock: prevent overlapping payments
            if (!await _paymentGate.WaitAsync(0))
                return;

            try
            {
                if (ViewModel.Item?.IsPaid == true)
                {
                    ShowMessage("خطا", "قبلا پرداخت شده");
                    return;
                }

                if (!PaymentPermission)
                    return;

                EnsureGateName();

                var snap = SnapshotPaymentState();

                TicketPaidInfoModel model;

                if (snap.totalAmount > 1000)
                {
                    model = new TicketPaidInfoModel
                    {
                        TicketId = snap.ticketId,
                        TotalAmount = snap.totalAmount,
                        PaidAmount = snap.paidAmount,
                        PaidCreditCard = "",
                        PaidType = "Naghdi",
                        RefId = "0000",
                        MerchantNumber = "00",
                        PaidDate = DateTime.Now.ToString("yyyyMMdd"),
                        RRN = "000",
                        TraceNo = "00000",
                        ExitGate = GateName,
                        IsMissingCard = snap.isMissing,
                        CardUid = snap.cardUid,
                        ExitRegistrarUserId = snap.userId,
                        ExitImage = snap.exitImage,
                        IsCustomPaid = true
                    };
                }
                else
                {
                    model = new TicketPaidInfoModel
                    {
                        TicketId = snap.ticketId,
                        TotalAmount = snap.totalAmount,
                        PaidAmount = snap.paidAmount,
                        PaidCreditCard = "",
                        PaidType = "Naghdi",
                        RefId = "0000",
                        MerchantNumber = "00",
                        PaidDate = DateTime.Now.ToString("yyyyMMdd"),
                        RRN = "000",
                        TraceNo = "00000",
                        ExitGate = GateName,
                        IsMissingCard = snap.isMissing,
                        CardUid = snap.cardUid,
                        ExitImage = snap.exitImage,
                        IsCustomPaid = true
                    };
                }

                await _parkingService.SetTicketPaidInfo(model);

                SaveExtraImages();
                await SetTicketData(snap.ticketId);
                SetPaymentStatus(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CustomPayment failed");
                ShowMessage("خطا", "خطا در پرداخت با مبلغ دلخواه");
            }
            finally
            {
                _paymentGate.Release();
            }
        }

        [RequiresPermission("CustomAmouontPayment", "خروج با مبلغ دلخواه")]
        private async void CustomPayment_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("CustomAmouontPayment"))
                return;

            var modal = new CustomAmountPaymentModalWindow(ViewModel.TicketId)
            {
                Owner = this
            };

            var result = modal.ShowDialog();
            if (result == true)
            {
                ViewModel.Item.PaidAmount = modal.ViewModel.Amount;
                await CustomPayment();
            }
        }

        private void AddImageListToExtraImageBox(List<(ImageSource imageSource, string title, bool ForSave)> list)
        {
            try
            {
                foreach (var image in list)
                {
                    var border = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(16, 206, 206, 206)),
                        Background = Brushes.Transparent,
                        Margin = new Thickness { Bottom = 0, Left = 5, Right = 5, Top = 5 }
                    };

                    var grid = new Grid();

                    var imageControl = new Image
                    {
                        Stretch = Stretch.Fill,
                        Source = image.imageSource,
                        Height = 200
                    };

                    var overlayPanel = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
                    };

                    var textBlock = new TextBlock
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

        private void ExitBtn_Click(object sender, RoutedEventArgs e) => Close();

        [RequiresPermission("PrintTicket", "چاپ قبض")]
        private void Print_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("PrintTicket"))
                return;

            PrintTicket();
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
                    EndTime = ((ticket.IsExited ?? false) && ticket.EndTime != null)
                        ? ticket.EndTime?.ToShamsi() + " " + ticket.EndTime?.ToString("HH:mm")
                        : "",
                    PaidAmount = ticket.PaidAmount.ToString("N0"),
                    TotalAmount = ticket.TotalAmount.ToString("N0"),
                    TotalDiscount = ticket.Discount.ToString("N0")
                }, widthPixels);

                DirectPrint(receiptContent);
            }
            catch
            {
                ShowMessage("خطا در پرینت", "خطا");
            }
        }

        private void DirectPrint(UIElement contentToPrint)
        {
            PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            double dpi = Settings.Default.Application_Print_dpi;
            double widthMm = Settings.Default.Application_Print_widthMm;
            double widthInches = widthMm / 25.4;
            double widthPixels = dpi * widthInches;

            contentToPrint.Measure(new Size(widthPixels, double.PositiveInfinity));
            contentToPrint.Arrange(new Rect(new System.Windows.Point(0, 0), contentToPrint.DesiredSize));
            double contentHeight = contentToPrint.DesiredSize.Height;

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
                Close();
            }
        }

        private void ShowPaymentLoader()
        {
            Dispatcher.Invoke(() =>
            {
                PaymentBtn.IsEnabled = false;
                PaymentBtnTextPanel.Visibility = Visibility.Collapsed;
                PaymentBtnLoader.Visibility = Visibility.Visible;
            });
        }

        private void HidePaymentLoader()
        {
            Dispatcher.Invoke(() =>
            {
                PaymentBtn.IsEnabled = true;
                PaymentBtnTextPanel.Visibility = Visibility.Visible;
                PaymentBtnLoader.Visibility = Visibility.Collapsed;
            });
        }
    }
}
