using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Parking.App.Views.Windows;

public partial class CustomAmountPaymentModalWindow : FluentWindow
{
    public CustomAmountPaymentViewModel ViewModel { get; set; } = new CustomAmountPaymentViewModel();
    private static readonly Regex _numericRegex = new Regex("[^0-9]+");

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

    private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = _numericRegex.IsMatch(e.Text);
    }

    private void AmountTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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
