using Parking.App.Models.Tickets;
using Parking.App.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;


namespace Parking.App.Views.Components;


public partial class TicketListView : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            "ItemsSource", 
            typeof(ObservableCollection<TicketsListViewModel>), 
            typeof(TicketListView), 
            new PropertyMetadata(null) 
        );

    public ObservableCollection<TicketsListViewModel> ItemsSource
    {
        get => (ObservableCollection<TicketsListViewModel>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public TicketListView()
    {
        InitializeComponent();
    }


    private void TicketList_Selected(object sender, RoutedEventArgs e)
    {
        try
        {
            var parent = this.Parent as FrameworkElement;
            MainPageViewModel parentValue = new MainPageViewModel();
            if (parent != null)
            {
                parentValue = parent.DataContext as MainPageViewModel;
            }


            if (TicketList.SelectedItem != null)
            {
                var ticket = TicketList.SelectedItem as TicketsListViewModel;
                var Details = new TicketDetailsWindow(ticket.Id, parentValue.CurrentFrame, null, null);
                Details?.Show();
                TicketList.SelectedItem = null;
            }

        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                Wpf.Ui.Controls.MessageBox ms = new Wpf.Ui.Controls.MessageBox();
                ms.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                ms.Title = "خطا";
                ms.Content = "خطا در نمایش قبض، لطفا مجددا تلاش نمایید";
                ms.IsPrimaryButtonEnabled = false;
                ms.IsSecondaryButtonEnabled = false;
                ms.CloseButtonText = "متوجه شدم";
                await ms.ShowDialogAsync();
            });
        }

    }
}
