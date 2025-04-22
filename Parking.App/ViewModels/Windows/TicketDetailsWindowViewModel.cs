using CommunityToolkit.Mvvm.ComponentModel;
using Parking.App.Models.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Windows;

public class TicketDetailsWindowViewModel : INotifyPropertyChanged
{
    private Guid ticketId;

    public Guid TicketId
    {
        get => ticketId;
        set
        {
            ticketId = value;
            OnPropertyChanged(nameof(TicketId));
        }
    }
    private string title;

    public string Title
    {
        get => title;
        set
        {
            title = value;
            OnPropertyChanged(nameof(Title));
        }
    }
    private TicketsListViewModel item;
    public TicketsListViewModel Item
    {
        get => item;
        set
        {
            item = value;
            OnPropertyChanged(nameof(Item));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
