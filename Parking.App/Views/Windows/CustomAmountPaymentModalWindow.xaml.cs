using System.Globalization;
using System.Windows.Controls;

namespace Parking.App.Views.Windows;

public partial class CustomAmountPaymentModalWindow : FluentWindow
{
    public CustomAmountPaymentViewModel ViewModel { get; set; } = new CustomAmountPaymentViewModel();

    public CustomAmountPaymentModalWindow(Guid ticketId)
    {
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.TicketId = ticketId;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var txt = AmountTextBox.Text.Replace(",", "");

        if (decimal.TryParse(txt, out decimal value))
        {
            int selectionStart = AmountTextBox.SelectionStart;

            AmountTextBox.TextChanged -= AmountTextBox_TextChanged;
            AmountTextBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", value);
            AmountTextBox.SelectionStart = AmountTextBox.Text.Length;
            AmountTextBox.TextChanged += AmountTextBox_TextChanged;

            ViewModel.Amount = value;
        }
        else
        {
            ViewModel.Amount = 0;
        }
    }
}
