using Nager.VideoStream;
using Parking.App.ANPR;
using Parking.App.Models.Dto.Card;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.Domain.General;
using System.Text.RegularExpressions;
using static Parking.App.ANPR.SATPA_API;


namespace Parking.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page

    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<MainPage> _logger;
        private DispatcherTimer _refreshDataTimer;
        private static CancellationTokenSource LocalCancellationTokenSource { get; set; } = new CancellationTokenSource();
        public MainPageViewModel ViewModel { get; private set; } = new MainPageViewModel();

        public MainPage()
        {
            _parkingService = App.GetService<IParkingService>();
            _logger = App.GetService<ILogger<MainPage>>();
            this.DataContext = ViewModel;
            InitializeComponent();
            LoadData();
            OtherPlateToggle.IsChecked = false;
            OtherPlateToggle_Unchecked(OtherPlateToggle, new RoutedEventArgs());
            LocalCancellationTokenSource = new CancellationTokenSource();
            this.Loaded += BarcodePage_Loaded;
            this.Unloaded += Page_Unloaded;
            this.PreviewKeyUp += Window_PreviewKeyUp;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("پنجره در حال بسته شدن است.");
        }

        private void BarcodePage_Loaded(object sender, RoutedEventArgs e)
        {
            BarcodeSearchBox.Visibility =
                Settings.Default.Appearance_ShowBarcodeSearchBox
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            ModifierKeys modifiers = Keyboard.Modifiers;

            if (IsShortcutMatched(key, modifiers))
            {
                e.Handled = true;
                ResetForm();
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var focused = Keyboard.FocusedElement;

                if (focused == BarcodeTextBox)
                {
                    BTNSearchBarcode_Click(BarcodeTextBox, new RoutedEventArgs());
                    e.Handled = true;
                }
                else
                {
                    CreateTicket();
                    e.Handled = true;
                }
            }
        }

        private bool IsShortcutMatched(Key pressedKey, ModifierKeys pressedModifiers)
        {
            string pressedShortcutText = pressedModifiers == ModifierKeys.None
                ? $"{pressedKey}"
                : $"{pressedModifiers} + {pressedKey}";

            string savedShortcutText = Settings.Default.Application_MainPage_ReloadShortcut;

            return pressedShortcutText == savedShortcutText;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_client != null)
            {
                _client = null;
                LocalCancellationTokenSource.Cancel();

            }
            if (satpa_object != null)
            {
                satpa_object.stop();
                satpa_object.stop_auto_process();
                satpa_object = null;
            }
            if (nfc != null)
            {
                nfc.CardUidReceived -= OnCardUidReceivedSlot;
                nfc = null;
            }
        }
        private async void LoadData()
        {
            PagePreparation();
            InitializeRefreshDataTimer();
            InitializeCardReader();
            InitializeRefreshExtraImagesTimer();
            await Task.Run(() =>
            {
                InitializeCamera();
            });
        }
        private void InitializeRefreshDataTimer()
        {
            if (Settings.Default.Appearance_UpdateListInterval > 0)
            {
                _refreshDataTimer = new DispatcherTimer();
                _refreshDataTimer.Interval = TimeSpan.FromSeconds(Settings.Default.Appearance_UpdateListInterval);
                _refreshDataTimer.Tick += RefreshDataTimer_Tick;
                _refreshDataTimer.Start();
            }
        }
        private async Task RefreshDataTimerAsync()
        {
            try
            {
                if (Settings.Default.Appearance_ShowLatestEntry)
                {
                    var entries = await _parkingService.GetLatestTicketsAsync(TicketType.Entrance, 10);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ViewModel.LatestEntryListItems = new ObservableCollection<TicketsListViewModel>(entries);
                    });
                }

                if (Settings.Default.Appearance_ShowLatestExited)
                {
                    var exits = await _parkingService.GetLatestTicketsAsync(TicketType.Exit, 10);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ViewModel.LatestExitedListItems = new ObservableCollection<TicketsListViewModel>(exits);
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        // Then for the event handler
        private void RefreshDataTimer_Tick(object sender, EventArgs e)
        {
            _ = RefreshDataTimerAsync(); // Fire and forget, but better to handle exceptions
        }
        private void PagePreparation()
        {
            #region مدیریت ستون های ورودی و خروجی
            int columnCount = 1;
            if (Settings.Default.Appearance_ShowLatestEntry)
            {
                LatestEntriesColumn.Visibility = Visibility.Visible;
                LatestEntriesColumn.SetValue(System.Windows.Controls.Grid.ColumnProperty, columnCount);
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                ViewModel.LatestEntryListItems = new ObservableCollection<TicketsListViewModel>(_parkingService.GetLatestTickets(TicketType.Entrance, 10));
            }
            if (Settings.Default.Appearance_ShowLatestExited)
            {
                LatestExitedColumn.Visibility = Visibility.Visible;
                LatestExitedColumn.SetValue(System.Windows.Controls.Grid.ColumnProperty, columnCount + 1);
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

                ViewModel.LatestExitedListItems = new ObservableCollection<TicketsListViewModel>(_parkingService.GetLatestTickets(TicketType.Exit, 10));
            }
            #endregion

            #region لود کردن لیست تعرفه
            int defaultVehicleSegmentId = Settings.Default.Application_DefaultVehicleSegmentPrice;
            var vehicleSegmentsList = _parkingService
                .GetVehicleSegments()
                .OrderByDescending(x => x.Id == defaultVehicleSegmentId)
                .ThenBy(x => x.Id == defaultVehicleSegmentId)
                .Select(v => new ComboBoxItem { Tag = v.Id, Content = v.NameFa })
                .ToList();

            foreach (var item in vehicleSegmentsList)
                VehicleSegmentComboBox.Items.Add(item);
            #endregion

            #region لود کردن لیست حروف پلاک
            var plateChars = LicensePlateHelper.GetChars();
            plateCharsCombo.ItemsSource = plateChars.Select(p => p.PlateFa).ToList();
            #endregion
        }
        #region سوئیچ ها قسمت ثبت قبض
        private void CustomDateToggle_Checked(object sender, RoutedEventArgs e)
        {
            minutesTextbox.Text = DateTime.Now.Minute.ToString();
            hourTextbox.Text = DateTime.Now.Hour.ToString();
            var time = DateTime.Now.ToShamsi().Split("/");
            dayTextbox.Text = time[2];
            monthTextbox.Text = time[1];
            yearTextbox.Text = time[0];

            timeBox.Visibility = Visibility.Visible;

            IsCustomTime = true;
        }

        private void CustomDateToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.DriverDescription = string.Empty;
            ViewModel.DriverPhoneNumber = string.Empty;
            ViewModel.DriverFullName = string.Empty;
            timeBox.Visibility = Visibility.Collapsed;
            IsCustomTime = false;
        }

        private void ExtraInfoToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            if (toggle.IsChecked == true)
            {
                extraInfoBox.Visibility = Visibility.Visible;
            }
            else
            {
                extraInfoBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OtherPlateToggle_Checked(object sender, RoutedEventArgs e)
        {
            ResetFormValues();
            IRPlateBox.Visibility = Visibility.Collapsed;
            OtherPlateBox.Visibility = Visibility.Visible;
            PlateTitle.Text = "پلاک منطقه، خارجی و یا موتور";
            RunPlateSort(true);
        }

        private void OtherPlateToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetFormValues();
            IRPlateBox.Visibility = Visibility.Visible;
            OtherPlateBox.Visibility = Visibility.Collapsed;
            PlateTitle.Text = "پلاک ایران";
            RunPlateSort(false);
        }
        private void RunPlateSort(bool isOtherPlate)
        {
            PlateType plateType = isOtherPlate ? PlateType.Other : PlateType.IranianPlate;

            var vehicleSegments = _parkingService.GetVehicleSegments();

            SortSegmentsByDetectedPlate(vehicleSegments, plateType);
        }


        #endregion

        #region دوربین و پلاکخوان

        public bool IsIranPlate { get; set; } = true;
        public string LatestValidEnPlate { get; set; } = "";
        public string LatestCreatedTicketEnPlate { get; set; } = "";
        private string LatestValidCarImage { get; set; }
        private bool ContinueProcessing { get; set; } = true;
        private bool IsValidPlate { get; set; }

        SATPA satpa_object = null;
        satpa_EVENT_CALLBACK HandleANPREventsDelegate = null;

        private async void InitializeCamera()
        {
            if (Settings.Default.Camera_MainCameraEnable)
            {
                this.Dispatcher.Invoke(() =>
                {
                    CamViewBox.Visibility = Visibility.Visible;
                    plateBorder.Visibility = Visibility.Collapsed;
                });

                if (Settings.Default.Camera_ANPR_Enable)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        imageBox.Visibility = Visibility.Visible;
                        plateBorder.Visibility = Visibility.Visible;
                    });

                    //VideoView.Visibility = Visibility.Collapsed;
                    HandleANPREventsDelegate = new satpa_EVENT_CALLBACK(HandleANPREvents);
                    satpa_set_event_callback(HandleANPREventsDelegate);

                    try
                    {

                        PictureBox pb = new PictureBox();
                        satpa_object = new SATPA(0, "cam1", pb, License.per_camera);
                        float cnf = ((float)Settings.Default.Camera_ANPR_Cnf) / 100;


                        SLPRPropertyGrid propSettings = new SLPRPropertyGrid();
                        propSettings.detect_persian_plate = 1;
                        propSettings.detect_english_plate = (Settings.Default.Application_DetectLatinPlate ? byte.Parse("1") : byte.Parse("0"));
                        propSettings.num_valid_chars = new int[] { 8, 5 };
                        propSettings.n_frm_skip_on_success = Settings.Default.Camera_ANPR_FrameSkip;
                        propSettings.vlc_net_cache_time = Settings.Default.Camera_ANPR_VlcCache;
                        propSettings.plate_type = Settings.Default.Camera_ANPR_PlateType;
                        propSettings.economy = (Settings.Default.Camera_ANPR_Economy) ? byte.Parse("1") : byte.Parse("0");
                        propSettings.diff_thresh = Settings.Default.Camera_ANPR_LightParameter;
                        propSettings.plate_buf_size = Settings.Default.Camera_ANPR_PlateCountInBuffer;
                        propSettings.min_cnf = (cnf > 1) ? 1 : cnf;

                        satpa_object.satpa_settings = propSettings;
                        satpa_object.url = Settings.Default.Camera_MainCameraUrl;
                        if (satpa_object.grabbing == "no")
                        {
                            satpa_object.play_mode = "vlc";
                            satpa_object.play(Settings.Default.Camera_MainCameraUrl);
                        }
                        else
                        {
                            satpa_object.satpa_settings.repeat = false;
                            satpa_object.stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("خطا", "خطا در بارگزاری ماژول پردازنده پلاک");
                    }

                }
                else
                {
                    await StartStream();
                }
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    CamViewBox.Visibility = Visibility.Collapsed;
                });

            }
        }
        protected async void HandleANPREvents(int event_type, byte stream, int plt_idx)
        {
            try
            {
                if (satpa_object != null && !App.GlobalCancellationTokenSource.IsCancellationRequested)
                {
                    if (event_type == WM_CAM_NOT_FOUND)
                        ShowMessage("خطا", "دوربین یافت نشد");
                    if (event_type == WM_PLATE_NOT_DETECTED)
                    {
                        //satpa_object.not_detect_counter++;
                        //if (satpa_object.grabbing == "no")
                        //    return;
                        //satpa_object.palteNotDetected(plt_idx);
                        //report_notdetected(stream);
                        //Console.WriteLine("\nnot detected");
                    }

                    else if (event_type == WM_NEW_FRAME)
                    {

                        short result = satpa_get_frame_info(stream, ref satpa_object.frame_w, ref satpa_object.frame_h, ref satpa_object.frame_ch, ref satpa_object.frame_step);
                        if (satpa_object.frame_w < 1)
                        {
                            satpa_object.stop();
                            ShowMessage("خطا", "ارتباط با دوربین " + satpa_object.name + "  به درستی برقرار نشده، دوباره تلاش کنید");
                            return;
                        }
                        if (result == 0)
                        {
                            if (satpa_object != null && satpa_object.get_frame() != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    ViewModel.CurrentFrame = satpa_object?.get_frame().ConvertBitmapToBitmapSource();
                                }));
                            }
                        }
                        else
                        {
                            ShowMessage("خطا", "خطا در دریافت تصویر");
                        }

                    }
                    //var u = WM_USER;
                    //var x = WM_PLATE_DETECTED;

                    if (event_type == WM_PLATE_DETECTED)
                    {
                        // در این قسمت باید اطلاعات پلاک با اندیس و دوربین اعلام شده را دریافت کنید
                        // برای دریافت اطلاعات از ترد جداگانه استفاده نکنید
                        // پس از دریافت اطلاعات پلاک برای ثبت در پایگاه داده می توانید از ترد جداگانه ای استفاده کنید
                        update(stream, plt_idx);
                        //your sample code
                    }
                    else if (event_type == WM_CONNECTED)
                    {
                        satpa_get_frame_info(stream, ref satpa_object.frame_w, ref satpa_object.frame_h, ref satpa_object.frame_ch, ref satpa_object.frame_step);
                        if (satpa_object.frame_w < 1)
                        {
                            satpa_object.stop();
                            System.Windows.MessageBox.Show("ارتباط با دوربین " + satpa_object.name + "  به درستی برقرار نشده، دوباره تلاش کنید");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("خطا", "خطا در ارتباط با پردازشگر تصویر");
                _logger.LogError(ex.Message, ex);
            }


        }
        private void report_notdetected(byte stream)
        {
            for (int i = 0; i < satpa_object.missed_buffer.Count(); i++)
            {
                missed_car car = satpa_object.missed_buffer[i];
                satpa_object.missed_buffer.RemoveAt(i);
                if (satpa_object.save)
                {
                    string t = (DateTime.Now.ToFileTime()).ToString();
                    car.frame.Save(satpa_object.name + "_no_plate" + "//" + t + ".jpg");
                    // new_plate.car_pic.Save(satpa_object.name + "//" + t + "_c_" + new_plate.result_en + ".jpg");
                }
            }
        }

        void update(byte stream, int plt_idx)
        {
            satpa_object.UpdateResults(plt_idx);
            report(stream);
        }
        private void report(byte stream)
        {

            for (int i = 0; i < satpa_object.plte_buffer.Count(); i++)
            {
                plate new_plate = satpa_object.plte_buffer[i];
                satpa_object.plte_buffer.RemoveAt(i);


                if (ContinueProcessing)
                {

                    if (new_plate.cnf >= ((float)Settings.Default.Camera_ANPR_Cnf / 100))
                    {
                        if (new_plate.splate_result.n_letter == 1 && new_plate.splate_result.n_char == 8)
                        {
                            IsIranPlate = true;


                            this.Dispatcher.Invoke(() =>
                            {
                                IRPlateBox.Visibility = Visibility.Visible;
                                OtherPlateToggle.IsChecked = false;
                                OtherPlateBox.Visibility = Visibility.Collapsed;
                            });
                            var plate = new_plate.splate_result.plate_english_string.Split("-");
                            LatestValidEnPlate = $"{plate[0]}_{plate[1].ToLower()}_{plate[2].Substring(0, 3)}_IR{plate[2].Substring(3, 2)}";
                            //_LatestValidFaLicensePlate = "ایران" + plate[2].Substring(3, 2) + "_" + plate[2].Substring(0, 3) + plate[1].ToLower()?.ConvertEnCharToFaCharIndex().Replace("ه", "هـ") + $"{plate[0]}";

                            if (LatestCreatedTicketEnPlate != LatestValidEnPlate)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    leftNumbersNumberTextBox.Text = plate[0];
                                    plateCharsCombo.SelectedValue = plate[1].ToLower().ConvertEnCharToFaCharIndex();
                                    rightNumbersNumberTextBox.Text = plate[2].Substring(0, 3);
                                    irNumberTextBox.Text = plate[2].Substring(3, 2);
                                    Keyboard.Focus(Application.Current.MainWindow);
                                });
                                CheckPlate();
                                LogHelper.LogDetectedPlate(new_plate.plate_pic.BitmapToBase64(), new_plate.car_pic.ResizeAndCompressBitmap(800, 600, 65, 65, 50), LatestValidEnPlate, "License Plate Detected.", _logger);
                                plateImageBox.Dispatcher.Invoke(() =>
                                {
                                    plateImageBox.ImageSource = new_plate.plate_pic.BitmapToImageSource();
                                    plateBorder.Background = new SolidColorBrush(Colors.Green);
                                });
                                LatestValidCarImage = new_plate.car_pic.ResizeAndCompressBitmap(1024, 768, 72, 72, 65);
                            }

                        }
                        else if (new_plate.splate_result.n_letter == 0)
                        {
                            IsIranPlate = false;
                            LatestValidEnPlate = new_plate.result_en;
                            this.Dispatcher.Invoke(() =>
                            {
                                IRPlateBox.Visibility = Visibility.Collapsed;
                                OtherPlateBox.Visibility = Visibility.Visible;
                                OtherPlateTextBox.Text = new_plate.result_en;
                                OtherPlateToggle.IsChecked = true;
                                CheckPlate();
                            });
                            plateImageBox.Dispatcher.Invoke(() =>
                            {
                                plateImageBox.ImageSource = new_plate.plate_pic.BitmapToImageSource();
                                plateBorder.Background = new SolidColorBrush(Colors.Green);
                            });
                            LatestValidCarImage = new_plate.car_pic.ResizeAndCompressBitmap(1024, 768, 72, 72, 65);
                        }
                    }

                }

            }

        }

        private void SortSegmentsByDetectedPlate(List<VehicleSegmentModel> vehicleSegments, PlateType plateType)
        {
            var segment = vehicleSegments.FirstOrDefault(x => x.PlateType == plateType);

            this.Dispatcher.Invoke(() =>
            {

                var vehicleSegmentsList = vehicleSegments.Where(x => x.PlateType == plateType || x.PlateType == Domain.General.PlateType.All)
                    .Select(v => new ComboBoxItem
                    {
                        Tag = v.Id,
                        Content = v.NameFa
                    }).ToList();



                if (vehicleSegmentsList != null)
                {
                    VehicleSegmentComboBox.Items.Clear();
                    int defaultVehicleSegmentId = Settings.Default.Application_DefaultVehicleSegmentPrice;

                    if (defaultVehicleSegmentId == 0)
                        vehicleSegmentsList.Insert(0, new ComboBoxItem { Tag = null, Content = "انتخاب کنید" });

                    foreach (var item in vehicleSegmentsList.OrderBy(x => x.Tag))
                        VehicleSegmentComboBox.Items.Add(item);

                    var vehicleSegment = vehicleSegmentsList.FirstOrDefault();
                    VehicleSegmentComboBox.SelectedIndex = vehicleSegmentsList.IndexOf(vehicleSegment);

                    VehicleSegmentId = int.TryParse(vehicleSegment?.Tag?.ToString(), out var id)
                        ? id
                        : 0;

                    VehicleSegmentName = VehicleSegmentComboBox.Text;

                    ViewModel.SelectedVehicleSegmentItem = new ComboBoxItem
                    {
                        Content = VehicleSegmentComboBox.SelectedIndex,
                        Tag = VehicleSegmentComboBox.SelectedIndex
                    };
                }
            });
        }

        private void CheckPlate()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (IsIranPlate && irNumberTextBox.Text != null && rightNumbersNumberTextBox.Text != null && leftNumbersNumberTextBox.Text != null)
                {
                    var letter = plateCharsCombo.SelectedItem as string;

                    LatestValidEnPlate = $"{leftNumbersNumberTextBox.Text}_{letter.ConvertFaCharToEnCharIndex()}_{rightNumbersNumberTextBox.Text}_IR{irNumberTextBox.Text}";
                }
                plateCheck.Visibility = Visibility.Collapsed;
            });

            if (IsIranPlate)
            {

                if (LatestValidEnPlate != null && LatestValidEnPlate.Length > 4)
                {

                    if (LicensePlateHelper.IsValidCarEnLicensePlateFormat(LatestValidEnPlate))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            plateCheck.Visibility = Visibility.Visible;
                            plateCheck.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(28, 47, 203, 44));
                            platecheckText.Text = "فرمت پلاک درست است";
                            platecheckText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 203, 44)); //rgb(47,203,44)

                        });
                        IsValidPlate = true;

                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            plateCheck.Visibility = Visibility.Visible;
                            plateCheck.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(28, 218, 50, 44));
                            platecheckText.Text = "پلاک نامعتبر";
                            platecheckText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(218, 50, 44));   //rgb(218,50,44)

                        });
                        IsValidPlate = false;
                    }
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        plateCheck.Visibility = Visibility.Visible;
                        plateCheck.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(28, 218, 50, 44));
                        platecheckText.Text = "پلاک نامعتبر";
                        platecheckText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(218, 50, 44));
                    });
                    IsValidPlate = false;
                }
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    plateCheck.Visibility = Visibility.Visible;
                    plateCheck.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(28, 47, 203, 44));
                    platecheckText.Text = "فرمت پلاک درست است";
                    platecheckText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 203, 44)); //rgb(47,203,44)

                });
                IsValidPlate = true;
            }

        }
        private async Task StartStream()
        {

            //mediaElement.Source = new Uri("rtsp://admin:admin123@192.168.88.197:554/Streaming/Channels/101");
            //mediaElement.Play();
            //ViewModel.CurrentFrame = bitmap.ConvertBitmapToBitmapImage(); 
            this.Dispatcher.Invoke(() =>
            {
                imageBox.Visibility = Visibility.Visible;
                CamViewBox.Visibility = Visibility.Visible;
            });


            var inputSource = new StreamInputSource(Settings.Default.Camera_MainCameraUrl);
            _client = new VideoStreamClient();

            _client.NewImageReceived += OnNewImageReceived;

            await _client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, LocalCancellationTokenSource.Token).ConfigureAwait(false);

        }
        private VideoStreamClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        private async void OnNewImageReceived(byte[] imageData)
        {
            await Task.Run(async () =>
            {
                if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        ViewModel.CurrentFrame = imageData.ToImageSource();
                    });
                }
            }).ConfigureAwait(false);
        }


        #endregion

        #region کارت ریدر
        private long _cardSerialNo = 0;
        private NFC? nfc = null;
        private void InitializeCardReader()
        {
            if (Settings.Default.Application_EntryCardRequirement)
            {
                nfc = new NFC();
                try
                {
                    nfc.Init(0, false);
                    nfc.CardUidReceived += OnCardUidReceivedSlot;
                }
                catch
                {
                    ShowMessage("اخطار", "دستگاه کارتخوان در دسترس نمیباشد");
                    return;
                }
            }
        }
        private bool isProcessing = false;

        private void OnCardUidReceivedSlot(byte[] uid)
        {
            if (isProcessing)
            {
                return;
            }

            isProcessing = true;
            try
            {
                _cardSerialNo = uid.GetCardUID();
                CreateTicket();
                _cardSerialNo = 0;
            }
            finally
            {
                isProcessing = false;

            }
        }
        #endregion

        #region ایجاد قبض
        public DateTime? CreateDateTiem { get; set; }
        public bool IsCustomTime { get; set; }
        public bool IsCustomPlate { get; set; }

        public int VehicleSegmentId { get; set; }
        public string VehicleSegmentName { get; set; }
        public string EntranceGate { get; set; }
        private bool CreateTicket()
        {
            try
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Settings.Default.Application_DeviceId))
                    {
                        ShowMessage("خطا", "لطفا برای استفاده از خدمات قبض لطفا شناسه دستگاه را در بخش تنظیمات اپلیکیشن پر کنید");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error01", ex);
                    return false;
                }

                try
                {



                    if (Settings.Default.Application_EntryCardRequirement)
                    {
                        CardModel? card = _parkingService.GetCardInfo(_cardSerialNo);
                        if (card is null)
                        {
                            ShowMessage("خطا", "کارت یافت نشد");
                            return false;
                        }

                        if (card.EnLicensePlate != null && card.EnLicensePlate != LatestValidEnPlate)
                        {
                            ShowMessage("خطا", "پلاک ثبت شده با پلاک کارت مطابقت ندارد");
                            return false;
                        }

                        //چک کردن اکتیو بودن کارت
                        if (!_parkingService.CardActiveStatus(_cardSerialNo))
                        {
                            ShowMessage("خطا", "کارت نا معتبر میباشد. چنانچه کارت برای این پارکینگ است نسبت به ثبت آن اقدام فرمایید.");
                            return false;
                        }


                        //چک کردن خالی بودن کارت
                        if (Settings.Default.Application_EntryCardRequirement)
                        {
                            if (_parkingService.IsCardInUse(_cardSerialNo))
                            {
                                // ورودی=0   خروجی=1
                                if (Settings.Default.Application_GateType.ToString().Contains("1"))
                                {
                                    Guid? cardTicketId = _parkingService.GetNotExitedTicketIdByCardSerialNo(_cardSerialNo);
                                    if (cardTicketId != null)
                                    {
                                        //چک کردن پلاک ثبتی کارت با پلاک عکس
                                        if (Settings.Default.Application_PlateCheckInExitGate)
                                        {
                                            var ticket = _parkingService.GetActiveTicketByCard(_cardSerialNo);
                                            if (ticket?.EnLicensePlate != LatestValidEnPlate)
                                            {
                                                ShowMessage("خطا", "پلاک ثبت شده برای کارت با پلاک پردازش شده یکسان نمیباشد");
                                                return false;
                                            }
                                            else
                                            {
                                                ShowTicketDetails(cardTicketId ?? new Guid());
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            ShowTicketDetails(cardTicketId ?? new Guid());
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        ShowMessage("خطا", "لطفا با کارت دیگری تلاش نمایید");
                                        return false;
                                    }
                                }
                                else
                                {
                                    ShowMessage("خطا", "کارت در حال استفاده میباشد، لطفا برای ورود از یکی از گیت های خروجی، کارت را خالی نمایید");
                                    return false;
                                }
                            }
                            var cardSegment = _parkingService.GetCardVehicleSegment(_cardSerialNo);
                            if (cardSegment != null)
                            {
                                VehicleSegmentName = cardSegment.NameFa;
                                VehicleSegmentId = cardSegment.Id;
                            }
                        }
                    }
                    else
                    {
                        if (VehicleSegmentId is 0)
                        {
                            ShowMessage("نوع تعرفه", "انتخاب نوع تعرفه اجباری است");
                            return false;
                        }

                        //چک کردن پلاک
                        var plateTicketId = _parkingService.GetActiveLicensePlateTicketId(LatestValidEnPlate);
                        if (plateTicketId != null)
                        {
                            // ورودی=0   خروجی=1
                            if (Settings.Default.Application_GateType.ToString().Contains("1"))
                            {

                                ShowTicketDetails(plateTicketId ?? new Guid());
                                ResetForm();
                                return false;
                            }
                            else
                            {
                                ShowMessage("خطا", "ورود این پلاک قبلا ثبت شده است، لطفا از گیت های خروجی نسبت به خارج کردن پلاک اقدام نمایید");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error02", ex);
                    return false;
                }

                try
                {
                    //چک کردن درست بودن پلاک
                    if (!IsValidPlate)
                    {
                        ShowMessage("خطا", "پلاک بدرستی وارد نشده است");
                        return false;
                    }


                    // ورودی=0   خروجی=1
                    if (!Settings.Default.Application_GateType.ToString().Contains("0"))
                    {
                        ShowMessage("خطا", "این گیت دسترسی ورود ندارد");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error03", ex);
                    return false;
                }



                try
                {



                    //تخصیص فضای پارک 
                    var parkingSpace = _parkingService.GetOneFreeSpaceId();
                    if (parkingSpace.SpaceId == null)
                    {
                        ShowMessage("خطا", "ظرفیت پارکینگ تکمیل میباشد");
                        return false;
                    }

                    //66_gh_732_IR42
                    // ایجاد قبض

                    DateTime startTime = DateTime.Now;
                    if (IsCustomTime)
                    {
                        CheckTime();
                        if (CreateDateTiem != null)
                        {
                            startTime = CreateDateTiem ?? DateTime.Now;
                        }
                    }
                    var plate = LatestValidEnPlate.ParsePlate();
                    var FaPlate = LatestValidEnPlate;
                    if (plate.IsIranianPlate)
                        FaPlate = "ایران" + plate.IranCode.Replace("IR", "") + "_" + plate.RightThreeDigits + plate.Letter.ToLower()?.ConvertEnCharToFaCharIndex().Replace("ه", "هـ") + $"{plate.LeftTwoDigits}";



                    if (Settings.Default.Application_GatePCName?.Length < 3)
                        EntranceGate = Environment.MachineName;
                    else
                        EntranceGate = Settings.Default.Application_GatePCName ?? "Unknown Gate";
                    try
                    {
                        CreateParkingTicketModel ticketModel = new CreateParkingTicketModel()
                        {
                            LicensePlate = FaPlate,
                            CardUid = _cardSerialNo,
                            DriverDescription = ViewModel.DriverDescription,
                            DriverPhoneNumber = ViewModel.DriverPhoneNumber,
                            DriverFullName = ViewModel.DriverFullName,
                            ParkingSpaceID = parkingSpace.SpaceId ?? new Guid(),
                            ParkingSectionId = parkingSpace.SectionId ?? new Guid(),
                            EnLicensePlate = LatestValidEnPlate,
                            EntranceGate = EntranceGate,
                            StartTime = startTime,
                            VehicleSegmentId = VehicleSegmentId,
                            VehicleManufacturerName = VehicleSegmentName ?? "نامشخص",
                            CreatorUserId = TokenStore.UserId,
                        };
                        try
                        {
                            if (!Settings.Default.Camera_ANPR_Enable)
                            {
                                if (ViewModel.CurrentFrame != null)
                                    LatestValidCarImage = ViewModel.CurrentFrame.ResizeAndCompressBitmap(1024, 768, 65, 65, 50);
                            }

                            var ticketInfo = _parkingService.CreateTicket(ticketModel, LatestValidCarImage);
                            if (ticketInfo.Succeeded)
                            {
                                LatestCreatedTicketEnPlate = ticketModel.EnLicensePlate;
                                if (Settings.Default.Application_PrintInvoiceAfterEntry)
                                {
                                    var ticket = _parkingService.GetTicketDetails(ticketInfo.Result);

                                    if (ticket != null)
                                    {
                                        var receiptContent = ReceiptPrinter.GenerateReceiptContent(new ReceiptModel
                                        {
                                            BarcodeId = ticket.BarcodeId,
                                            Description = "--",
                                            LicensePlate = ticket.LicensePlate,
                                            ParkingName = ticket.ParkingName,
                                            StartTime = ticket.StartTime.ToLongShamsiString() + " " + ticket.StartTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                                            VehicleSegmentName = ticket.VehicleSegmentName
                                        });
                                        PrintHelper.Print(receiptContent);
                                    }

                                }
                                try
                                {
                                    SaveExtraImages(ticketInfo.Result);
                                    ShowCreatedTicketBox(ticketModel.StartTime, ticketModel.LicensePlate, ticketModel.VehicleManufacturerName);
                                    ResetForm();
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Error07", ex);
                                    return false;
                                }
                            }
                            else
                            {
                                ShowMessage("خطا", "خطا در ثبت قبض");
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error06", ex);
                            return false;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error05", ex);
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error04", ex);
                    return false;
                }



            }
            catch (Exception e)
            {
                _logger.LogError("Error In Create Ticket: " + e.Message, e);
                ShowMessage("خطا", "خطا در ثبت قبض");
                return false;
            }
        }

        private async void ShowCreatedTicketBox(DateTime dateTime, string plate, string price)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                TicketDetailsViewModel td = new TicketDetailsViewModel()
                {
                    EntryTime = dateTime.ToShortTimeString().Replace("AM", "ق.ظ").Replace("PM", "ب.ظ"),
                    PlateText = plate,
                    VehicleSegmentPrice = price
                };
                var ticketDetails = new TicketDetails();
                ticketDetails.SetContent(td);
                ticketDetails.StartSequence(5);
                TicketDetailsBox.Children.Add(ticketDetails);
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };

                timer.Tick += (sender, e) =>
                {
                    TicketDetailsBox.Children.Remove(ticketDetails);
                    timer.Stop();
                };

            });

        }

        public void ResetFormValues()
        {
            this.Dispatcher.Invoke(() =>
            {
                rightNumbersNumberTextBox.Text = string.Empty;
                leftNumbersNumberTextBox.Text = string.Empty;
                ViewModel.DriverDescription = string.Empty;
                ViewModel.DriverPhoneNumber = string.Empty;
                ViewModel.DriverFullName = string.Empty;
                IsCustomTime = false;
                plateCheck.Visibility = Visibility.Collapsed;
            });
        }

        public void ResetForm()
        {
            this.Dispatcher.Invoke(() =>
            {
                OtherPlateTextBox.Text = string.Empty;
                irNumberTextBox.Text = string.Empty;
                rightNumbersNumberTextBox.Text = string.Empty;
                leftNumbersNumberTextBox.Text = string.Empty;
                ContinueProcessing = true;
                OtherPlateToggle.IsChecked = false;
                customDateToggle.IsChecked = false;
                ViewModel.DriverDescription = string.Empty;
                ViewModel.DriverPhoneNumber = string.Empty;
                ViewModel.DriverFullName = string.Empty;
                IsCustomTime = false;
                plateCheck.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

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
                        //ms.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(252, 228, 236));
                        //ms.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 33, 33));
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

        private async void PlateTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ContinueProcessing = false;
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(20)
                };
                timer.Tick += (sender, e) =>
                {
                    ContinueProcessing = true;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PlateTextBox_GotFocus");
            }
        }

        private void VehicleSegmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = VehicleSegmentComboBox.SelectedItem as ComboBoxItem;
            if (selected != null)
            {
                VehicleSegmentId = (int?)selected?.Tag ?? 0;
                VehicleSegmentName = (string)selected?.Content;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPlate();
        }

        private void TimeTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (yearTextbox.Text != null &&
                          monthTextbox.Text != null &&
                          dayTextbox.Text != null &&
                          hourTextbox.Text != null &&
                          minutesTextbox.Text != null
                          )
                {
                    CreateDateTiem = DateConvertor
                          .ShamsiToDateTime(int.Parse(yearTextbox.Text),
                          int.Parse(monthTextbox.Text),
                          int.Parse(dayTextbox.Text),
                          int.Parse(hourTextbox.Text),
                          int.Parse(minutesTextbox.Text));
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void CheckTime()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (yearTextbox.Text == null)
                    ShowMessage("خطا", "سال وارد نشده است");
                if (monthTextbox.Text == null)
                    ShowMessage("خطا", "ماه وارد نشده است");
                if (dayTextbox.Text == null)
                    ShowMessage("خطا", "روز وارد نشده است");
                if (hourTextbox.Text == null)
                    ShowMessage("خطا", "ساعت وارد نشده است");
                if (minutesTextbox.Text == null)
                    ShowMessage("خطا", "دقیقه وارد نشده است");
            });
        }


        private void ShowTicketDetails(Guid cardTicketId)
        {
            this.Dispatcher.Invoke(() =>
            {
                var Details = new TicketDetailsWindow(cardTicketId, ViewModel.CurrentFrame, ExtraImagesList, _cardSerialNo);
                Details?.Show();
            });
        }

        private void leftNumbersNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (leftNumbersNumberTextBox.Text.Length == 2)
                plateCharsCombo.Focus();
            CheckPlate();
        }

        private void rightNumbersNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rightNumbersNumberTextBox.Text.Length == 3)
                irNumberTextBox.Focus();
            CheckPlate();
        }

        private void irNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (irNumberTextBox.Text.Length == 2)
                CheckPlate();

        }

        private void CreateTicketBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateTicket();
            }
            catch
            {
                ShowMessage("خطا", "خطا در ایجاد قبض، لطفا مجددا تلاش کنید");
            }
        }

        private void MainPage_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                //if (e.Key == Key.Enter)
                //{
                //    CreateTicket();
                //}
            }
            catch
            {
                ShowMessage("خطا", "خطا در ایجاد قبض، لطفا مجددا تلاش کنید");
            }
        }

        //[DllImport("kernel32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool AllocConsole();

        //public static void ShowConsole()
        //{
        //    AllocConsole();
        //    Console.WriteLine("========================>>>>>>>>>>>>>");
        //}
        #region ImageBox

        private DispatcherTimer _refreshExtraImagesTimer;
        private void InitializeRefreshExtraImagesTimer()
        {
            if (Settings.Default.Appearance_UpdateListInterval > 0)
            {
                _refreshDataTimer = new DispatcherTimer();
                _refreshDataTimer.Interval = TimeSpan.FromMilliseconds(2500);
                _refreshDataTimer.Tick += RefreshExtraImagesTimer_Tick;
                _refreshDataTimer.Start();
            }
        }
        private void RefreshExtraImagesTimer_Tick(object sender, EventArgs e)
        {
            //LoadExtraImages();
            UpdateCameraImages();
        }
        public List<CameraConfigModel> Cameras { get; set; } = CameraConfigManager.GetActiveCameras();
        private List<(string Image, string Name)> ExtraImagesList = new List<(string Image, string Name)>();
        private async void UpdateCameraImages()
        {
            try
            {
                List<(string Image, string Name)> result = new List<(string Image, string Name)>();
                foreach (var cam in Cameras)
                {
                    var bitmapImage = await FetchImage.FetchImageWithDigestAuthAsync(cam.SnapshotUrl, cam.Username ?? "", cam.Password ?? "");

                    var camera = ViewModel.CameraImages.FirstOrDefault(c => c.Name == cam.Name);
                    if (camera != null)
                    {
                        if (bitmapImage != null && bitmapImage.PixelWidth > 0 && bitmapImage.PixelHeight > 0)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                camera.ImageSource = bitmapImage;
                            }, DispatcherPriority.Background);

                            //camera.ImageSource = bitmapImage;
                            result.Add(new(bitmapImage.ResizeAndCompressBitmap(1024, 768, 72, 72, 60), cam.Name));
                        }
                    }
                    else
                    {
                        ViewModel.CameraImages.Add(new CameraImageModel { Name = cam.Name, ImageSource = bitmapImage });
                    }
                }
                if (result.Count == Cameras.Count)
                {
                    ExtraImagesList.Clear();
                    ExtraImagesList = result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }


        private void SaveExtraImages(Guid ticketId)
        {
            try
            {
                foreach (var image in ExtraImagesList)
                {
                    _parkingService?.AddTicketExtraImage(ticketId, image.Image, image.Name, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }
        #endregion

        private void OtherPlateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LatestValidEnPlate = OtherPlateTextBox.Text;
            CheckPlate();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private static readonly Regex _numericRegex = new Regex("[^0-9]+");

        private void BarcodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        private void BarcodeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_numericRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void BTNSearchBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(BarcodeTextBox.Text))
            {

                long barcode = long.Parse(BarcodeTextBox.Text);

                var ticketId = _parkingService.GetTicketIdByBarcode(barcode);
                if (ticketId is null || ticketId == Guid.Empty)
                {
                    ShowMessage("قبض", "قبضی با این بارکد یافت نشد");
                    return;
                }

                ShowTicketDetails(ticketId.Value);
            }
            return;
        }
    }

}
