namespace Parking.App.Views.Components;

/// <summary>
/// Interaction logic for SeizedPlateItemComponent.xaml
/// </summary>
public partial class SeizedPlateItemComponent : UserControl
{
    public SeizedPlateItemComponent()
    {
        InitializeComponent();
    }

    public static readonly RoutedEvent DeleteRequestedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(DeleteRequested),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SeizedPlateItemComponent));

    public event RoutedEventHandler DeleteRequested
    {
        add => AddHandler(DeleteRequestedEvent, value);
        remove => RemoveHandler(DeleteRequestedEvent, value);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SeizedLicensePlateModel model)
        {
            RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent, model));
        }
    }
}

