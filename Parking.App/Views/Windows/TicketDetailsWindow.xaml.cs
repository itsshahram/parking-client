


using Microsoft.EntityFrameworkCore.Metadata;
using Parking.App.Models.Config;
using Parking.App.Models.Dto.Parking.ParkingTicket;
using Parking.App.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Border = Wpf.Ui.Controls.Border;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Grid = Wpf.Ui.Controls.Grid;
using Image = Wpf.Ui.Controls.Image;
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
        private readonly IParkingService? _parkingService;
        private bool IsMissingCard { get; set; } = false;
        private bool PaymentPermission { get; set; } = true;
        private string ExitImage { get; set; }
        private List<(ImageSource ImageSource, string Name, bool ForSave)> ExtraImagesList = new List<(ImageSource ImageSource, string Name, bool ForSave)>();

        private DispatcherTimer _closeTimer;
        public TicketDetailsWindow(Guid ticketId, BitmapSource currentImage, List<(string Image, string Name)>? extraimages, long? cardUid)
        {
            try
            {

                _parkingService = App.GetService<IParkingService>();
                Cameras = CameraConfigManager.GetActiveCameras();

                if (ticketId != null)
                {
                    this.DataContext = ViewModel;
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

                    SetTicketData(ticketId);
                    
                    if (currentImage != null)
                    {
                        currentImg.Source = currentImage;
                        ExitImage = currentImage.ResizeAndCompressBitmap(1024, 768, 72, 72, 65);
                    }
                    if (extraimages != null) {

                        foreach(var image in extraimages)
                        {
                            if (image.Image != null)
                            {
                                ExtraImagesList.Add((ImageHelper.Base64ToImageSource(image.Image), image.Name, true));
                            }
                        }
                    }
                    if(cardUid != null)
                    {
                        if (cardUid == 0 && !PermissionHelper.CheckUserPermission(TokenStore.RoleName, "ForceExitRequest"))
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
            }


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

        private async void SetTicketData(Guid ticketId)
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
                    //MissingCardToggle.IsChecked = true;
                    MissingCardToggle.IsEnabled = false;
                }
                // ورودی=0   خروجی=1
                if (!Settings.Default.Application_GateType.ToString().Contains("1"))
                {
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    CashPaymentBtn.Visibility = Visibility.Collapsed;
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    MissingCardToggle.IsEnabled = false;
                }
                if (!PaymentPermission)
                {
                    PaymentBtn.Visibility = Visibility.Collapsed;
                    CashPaymentBtn.Visibility = Visibility.Collapsed;
                    PaymentBtn.Visibility = Visibility.Collapsed;
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
            plate_LeftNumber.Text = plate.LeftTwoDigits;
            plate_RightNumber.Text = plate.RightThreeDigits;
            plate_IRNumber.Text = plate.IranCode.Replace("IR", "");
            plate_Char.Text = plate.Letter.ConvertEnCharToFaCharIndex();
        }
        private void TicketDetailsWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if (Settings.Default.Application_GateType.Contains("1"))
            {
                if (e.Key == Key.F1)
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
                if (e.Key == Key.Space)
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

                if (e.Key == Key.F12)
                {
                    if (!ViewModel.Item?.IsPaid ?? false)
                    {
                        if (!IsMissingCard)
                        {
                            MissingCardToggle.IsChecked = true;
                        }
                        else
                        {
                            MissingCardToggle.IsChecked = false;
                        }
                    }
                    else
                    {
                        ShowMessage("توجه", "این قبض قبلا پرداخت شده، امکان پرداخت دوباره یا تغییر وجود ندارد");
                    }

                }


            }


        }
        public async void Payment()
        {
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
                                    CardUid = ViewModel.Item.CardUid
                                });
                                SetTicketData(ViewModel.Item.Id);
                                SetPaymentStatus(true);
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
                                CardUid = ViewModel.Item.CardUid
                            });
                            if (rs)
                            {

                                SetTicketData(ViewModel.Item.Id);
                                SetPaymentStatus(true);
                                SaveExtraImages();
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
        private void CashPayment_Click(object sender, RoutedEventArgs e)
        {
            CashPayment();
        }

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
                if (TokenStore.ServerStatus)
                {
                    var entryimage = await _parkingService.GetTicketImage(ticketId);
                    await this.Dispatcher.InvokeAsync(() => EntryImage.Source = entryimage);
                }
                
                var extraImages = await _parkingService.GetTicketExtraImageSourcesAsync(ticketId, true);
                if (extraImages != null)
                {
                    foreach (var image in extraImages)
                    {
                        if (image.ImageSource != null)
                        {
                            ExtraImagesList.Add(new(image.ImageSource , image.FaName??"_", false));
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
        private void PaymentBtn_Click(object sender, RoutedEventArgs e)
        {

            Payment();
        }
        private string GateName { get; set; }
        public async void CashPayment()
        {
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
                        if (ViewModel.Item.TotalAmount > 1000)
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
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
                                CardUid = ViewModel.Item.CardUid
                            });

                        }
                        else
                        {
                            var rs = _parkingService.SetTicketPaidInfo(new TicketPaidInfoModel()
                            {
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
                                CardUid = ViewModel.Item.CardUid
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


            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }


        }
        
        private void SaveExtraImages()
        {
            foreach (var image in ExtraImagesList.Where(e=>e.ForSave))
            {
                _parkingService?.AddTicketExtraImage(ViewModel.Item.Id, image.ImageSource.ImageSourceToBase64(), image.Name, true);
            }
        }
    }
}
