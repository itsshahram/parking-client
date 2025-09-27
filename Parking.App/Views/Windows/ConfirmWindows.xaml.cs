namespace Parking.App.Views.Windows;

public enum ConfirmType
{
    Default,
    Delete,
    Warning
}
public partial class ConfirmWindow : FluentWindow
{
    public bool IsConfirmed { get; private set; }

    public ConfirmWindow(
        string title,
        string message,
        ConfirmType type = ConfirmType.Default,
        string acceptText = "تأیید",
        string cancelText = "انصراف")
    {
        InitializeComponent();

        ConfirmTitle.Text = title;
        ConfirmMessage.Text = message;
        BtnAccept.Content = acceptText;
        BtnCancel.Content = cancelText;

        switch (type)
        {
            case ConfirmType.Delete:
                BtnAccept.Appearance = ControlAppearance.Danger;
                break;
            case ConfirmType.Warning:
                BtnAccept.Appearance = ControlAppearance.Caution; 
                break;
            default:
                BtnAccept.Appearance = ControlAppearance.Success; 
                break;
        }
    }

    private void BtnAccept_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        DialogResult = false;
        Close();
    }
}

