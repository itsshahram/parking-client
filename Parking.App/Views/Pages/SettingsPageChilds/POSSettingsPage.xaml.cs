using TextBox = Wpf.Ui.Controls.TextBox;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for POSSettingsPage.xaml
    /// </summary>
    public partial class POSSettingsPage : Page
    {
        public POSSettingsPage()
        {
            InitializeComponent();
        }

        private void SavePOSConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void POSTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            string type = selectedItem.Tag.ToString() ;

            Settings.Default.POS_Type = type;
            Settings.Default.Save();
        }

        private void Connection_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            string type = selectedItem.Tag.ToString();

            Settings.Default.POS_Connection_Type = type;
            Settings.Default.Save();
        }

        private void POSIPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.POS_IP = ((TextBox)sender).Text;
            Settings.Default.Save();
        }

        private void POSPortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
        private void POSPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(POSPortTextBox.Text, out int port))
            {
                if (port >= 0 && port <= 65535)
                {
                    POSPortTextBox.BorderBrush = new SolidColorBrush(Colors.Green);
                    Settings.Default.POS_Port = int.Parse(POSPortTextBox.Text);
                    Settings.Default.Save();
                }
                else
                {
                    POSPortTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                POSPortTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
