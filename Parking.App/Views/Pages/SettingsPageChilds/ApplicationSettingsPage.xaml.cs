
using Wpf.Ui.Violeta.Controls;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private readonly IParkingService _parkingService;
        public ApplicationSettingsPage()
        {

            _parkingService = App.GetService<IParkingService>();

            InitializeComponent();

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
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var publishDate = (BuildDateAttribute)Assembly
                                .GetExecutingAssembly()
                                .GetCustomAttributes(typeof(BuildDateAttribute), false)
                                .FirstOrDefault();
            AppVersionText.Text = version;
            PublishDateText.Text = publishDate?.Date.ToString() ?? "Unknown";

            APIServerAddressTextBox.Text = Settings.Default.Application_ApiServerAddress;
            this.PreviewKeyDown += Window_PreviewKeyDown;
        }
        private void Change_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
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

        private void DeviceId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!PermissionHelper.CheckUserPermission(TokenStore.RoleName, "ApplicationSettings"))
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
            if (!PermissionHelper.CheckUserPermission(TokenStore.RoleName, "ApplicationSettings"))
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
        private Key _pressedKey;
        private ModifierKeys _pressedModifiers;

        private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.None)
                key = e.ImeProcessedKey;

            _pressedModifiers = Keyboard.Modifiers;
            _pressedKey = key;

            // Format display text:
            // If no modifiers, show only key (e.g., "F5")
            // Otherwise show modifiers + key (e.g., "Control + F5")
            string shortcutText = _pressedModifiers == ModifierKeys.None
                ? $"{_pressedKey}"
                : $"{_pressedModifiers} + {_pressedKey}";

            ShortcutTextBox.Text = shortcutText;

            // Save shortcut string to settings
            Settings.Default.Application_MainPage_ReloadShortcut = shortcutText;
            Settings.Default.Save();
        }

        // Retrieve the shortcut later
        private (ModifierKeys modifiers, Key key) GetShortcut()
        {
            return (_pressedModifiers, _pressedKey);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ShortcutTextBox.IsFocused)
            {
                e.Handled = true;

                Key key = e.Key == Key.System ? e.SystemKey : e.Key;
                if (key == Key.None)
                    key = e.ImeProcessedKey;

                _pressedModifiers = Keyboard.Modifiers;
                _pressedKey = key;

                string shortcutText = _pressedModifiers == ModifierKeys.None
                    ? $"{_pressedKey}"
                    : $"{_pressedModifiers} + {_pressedKey}";

                ShortcutTextBox.Text = shortcutText;

                Settings.Default.Application_MainPage_ReloadShortcut = shortcutText;
                Settings.Default.Save();
            }
        }
    }
}
