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
            var parentValue = parent?.DataContext as MainPageViewModel;

            if (TicketList.SelectedItem is TicketsListViewModel ticket && parentValue != null)
            {
                var window = Window.GetWindow(this);
                var details = new TicketDetailsWindow(ticket.Id, parentValue.CurrentFrame, null, null);
                details.Owner = window;
                details?.ShowDialog();
                TicketList.SelectedItem = null;
            }
        }
        catch (Exception)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var ms = new Wpf.Ui.Controls.MessageBox
                {
                    FlowDirection = System.Windows.FlowDirection.RightToLeft,
                    Title = "خطا",
                    Content = "خطا در نمایش قبض، لطفا مجددا تلاش نمایید",
                    IsPrimaryButtonEnabled = false,
                    IsSecondaryButtonEnabled = false,
                    CloseButtonText = "متوجه شدم"
                };
                await ms.ShowDialogAsync();
            });
        }
    }
}
