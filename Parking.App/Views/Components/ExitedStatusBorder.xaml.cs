

namespace Parking.App.Views.Components
{
    /// <summary>
    /// Interaction logic for ExitedStatusBorder.xaml
    /// </summary>
    public partial class ExitedStatusBorder : UserControl
    {
        public ExitedStatusBorder()
        {
            InitializeComponent();
            UpdateUI();
        }
        public static readonly DependencyProperty IsExitedProperty =
            DependencyProperty.Register(
                "IsExited",
                typeof(bool),
                typeof(ExitedStatusBorder),
                new PropertyMetadata(false, OnIsExitedChanged));

        public bool IsExited
        {
            get => (bool)GetValue(IsExitedProperty);
            set => SetValue(IsExitedProperty, value);
        }

        private static void OnIsExitedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExitedStatusBorder statusBorder)
            {
                statusBorder.UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (IsExited)
            {
                MainBorder.Background = new SolidColorBrush(Colors.Green);
                StatusText.Text = "خارج شده است";
            }
            else
            {
                MainBorder.Background = new SolidColorBrush(Colors.Red);
                StatusText.Text = "خارج نشده است";
            }
        }


    }
}
