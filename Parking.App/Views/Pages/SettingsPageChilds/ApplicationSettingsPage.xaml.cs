using static Parking.App.Helpers.AppInfoHelper;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private readonly IParkingService _parkingService;
        public ObservableCollection<TicketDescriptionItemModel> Descriptions { get; set; } = new ObservableCollection<TicketDescriptionItemModel>();
        public ICommand RemoveDescriptionCommand { get; }

        public ApplicationSettingsPage()
        {
            RemoveDescriptionCommand = new Helpers.RelayCommand(RemoveDescription);

            _parkingService = App.GetService<IParkingService>();

            InitializeComponent();
            this.DataContext = this;
            var vehicleSegmentsList = _parkingService.GetVehicleSegments().Select(v => new ComboBoxItem { Tag = v.Id, Content = v.NameFa }).ToList();
            vehicleSegmentsList.Insert(0, new ComboBoxItem { Tag = 0, Content = "انتخاب بدون پیش ‌فرض" });
            foreach (var item in vehicleSegmentsList)
                VehicleSegmentComboBox.Items.Add(item);
            if (Settings.Default.Application_DefaultVehicleSegmentPrice > 0)
                VehicleSegmentComboBox.SelectedIndex = vehicleSegmentsList.IndexOf(vehicleSegmentsList.FirstOrDefault(v => (int)v.Tag == Settings.Default.Application_DefaultVehicleSegmentPrice));

            if (Settings.Default.Application_Logging_In_Elastic)
            {
                if (ElasticBox != null)
                    ElasticBox.Visibility = Visibility.Visible;
            }
            AppVersionText.Text = GetVersion();
            PublishDateText.Text = GetBuildDate();
            APIServerAddressTextBox.Text = Settings.Default.Application_ApiServerAddress;
            this.Unloaded += SyncConfigPage_Unloaded;

            LoadDescriptions();
        }
        private void LoadDescriptions()
        {
            var items = _parkingService.GetAllTicketDescriptionItems();
            Descriptions.Clear();
            foreach (var item in items)
            {
                Descriptions.Add(item);
            }
        }
        private void RemoveDescription_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow("حذف", "ایا از حذف این توضحیات مطمعن هستید ؟", ConfirmType.Delete, "حذف");
            if (confirm.ShowDialog() == true)
            {
                if (sender is System.Windows.Controls.Button btn && btn.DataContext is TicketDescriptionItemModel item)
                {
                    _parkingService.DeleteTicketDescriptionItem(item.Id);
                    LoadDescriptions();
                }
            }
        }

        private void RefreshQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is TicketDescriptionItemModel item)
            {
                ConfirmWindow confirm = new ConfirmWindow("بازنشانی", $"ایا از بازنشانی صف {item.Text} مطمعن هستید ؟", ConfirmType.Warning, "بازنشانی");
                if (confirm.ShowDialog() == true)
                {
                    _parkingService.ResetTicketDescriptionInterval(item.Id);
                    LoadDescriptions();
                }
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void SyncConfigPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("ApplicationSettings"))
                AllDeviceTicketsToggle.Visibility = Visibility.Visible;
            else
                AllDeviceTicketsToggle.Visibility = Visibility.Collapsed;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            Settings.Default.Application_GateType = selectedItem?.Tag?.ToString() ?? "0";
            Settings.Default.Save();
        }

        private void VehicleSegmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Settings.Default.Application_DefaultVehicleSegmentPrice = short.Parse(selectedItem.Tag.ToString());
                Settings.Default.Save();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Application_MissingCardPrice = decimal.Parse(((TextBox)sender).Text);
            Settings.Default.Save();
        }
        private void GateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Application_GatePCName = ((TextBox)sender).Text;
            Settings.Default.Save();
        }
        private void TextBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void DeviceId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("ApplicationSettings"))
            {
                System.Windows.MessageBox.Show("تنها مدیر پارکینگ می‌تواند این فیلد را تغییر دهد.");
                return;
            }
            Settings.Default.Application_DeviceId = ((TextBox)sender).Text;
            Settings.Default.Save();
        }

        private void elasticConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void ElasticToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (ElasticBox != null)
                ElasticBox.Visibility = Visibility.Visible;
        }

        private void ElasticToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ElasticBox != null)
                ElasticBox.Visibility = Visibility.Collapsed;
        }

        private void APIServerAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission("ApplicationSettings"))
            {
                System.Windows.MessageBox.Show("تنها مدیر پارکینگ می‌تواند این فیلد را تغییر دهد.");
                return;
            }
            if (((TextBox)sender).Text != null && ((TextBox)sender).Text.Length > 5)
            {
                Settings.Default.Application_ApiServerAddress = ((TextBox)sender).Text;
                Settings.Default.Save();
            }

        }

        private void DescriptionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddDescription();
                e.Handled = true;
            }
        }

        private void AddDescription()
        {
            var text = DescriptionTextBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                if (Descriptions.Any(d => d.Text == text))
                {
                    System.Windows.MessageBox.Show("این توضیح قبلا اضافه شده است.");
                    return;
                }
                var newItem = new TicketDescriptionItemModel
                {
                    Text = text,
                    CreateDate = DateTime.Now,
                    IsQueueEnabled = false
                };
                _parkingService.AddTicketDescriptionItem(newItem);
                LoadDescriptions();
            }
            DescriptionTextBox.Clear();
        }

        private void RemoveDescription(object param)
        {
            if (param is TicketDescriptionItemModel)
            {
                _parkingService.DeleteTicketDescriptionItem(((TicketDescriptionItemModel)param).Id);
                LoadDescriptions();
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggle && toggle.DataContext is TicketDescriptionItemModel item)
            {
                bool newValue = (bool)toggle.IsChecked;
                _parkingService.ChangeTicketDescriptionItemQueueStatus(item.Id, newValue);
            }
        }
    }
}
