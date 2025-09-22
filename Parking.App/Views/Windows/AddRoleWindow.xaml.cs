namespace Parking.App.Views.Windows;

/// <summary>
/// Interaction logic for AddRoleWindow.xaml
/// </summary>
public partial class AddRoleWindow : FluentWindow
{
    public AddRoleWindowViewModel ViewModel { get; private set; }
    public AddRoleWindow()
    {
        InitializeComponent();
        ViewModel = new AddRoleWindowViewModel();
        DataContext = ViewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
