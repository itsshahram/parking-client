using System.ComponentModel;

namespace Parking.App.ViewModels.Windows;

public class CustomAmountPaymentViewModel : INotifyPropertyChanged
{
    private Guid ticketId;
    private decimal amount;

    public Guid TicketId
    {
        get => ticketId;
        set
        {
            ticketId = value;
            OnPropertyChanged(nameof(TicketId));
        }
    }

    public decimal Amount
    {
        get => amount; set
        {
            amount = value;
            OnPropertyChanged(nameof(Amount));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
