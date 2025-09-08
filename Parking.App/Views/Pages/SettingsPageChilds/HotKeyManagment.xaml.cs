using MessageBox = Wpf.Ui.Controls.MessageBox;
using TextBox = Wpf.Ui.Controls.TextBox;
namespace Parking.App.Views.Pages.SettingsPageChilds;

public partial class HotKeyManagment : Page
{
    private readonly HotKeyManagementViewModel _vm;

    public HotKeyManagment()
    {
        InitializeComponent();
        _vm = new HotKeyManagementViewModel();
        DataContext = _vm;

        ApplyHotkeys();
    }

    private void SaveHotkeys_Click(object sender, RoutedEventArgs e)
    {
        _vm.Save();
        ApplyHotkeys();
        ShowMessage("کلید مبانبر", "کلید های میانبر ذخیره شد");
    }
    private async void ShowMessage(string title, string message)
    {
        try
        {
            if (!App.GlobalCancellationTokenSource.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    MessageBox ms = new MessageBox
                    {
                        FlowDirection = System.Windows.FlowDirection.RightToLeft,
                        Title = title,
                        Content = message,
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false,
                        CloseButtonText = "متوجه شدم"
                    };
                    await ms.ShowDialogAsync();
                });
            }
        }
        catch
        {
            System.Windows.MessageBox.Show(message, title);
        }
    }


    private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.None)
            key = e.ImeProcessedKey;

        ModifierKeys modifiers = Keyboard.Modifiers;

        string shortcutText = modifiers == ModifierKeys.None
            ? $"{key}"
            : $"{modifiers} + {key}";

        if (sender is TextBox tb)
        {
            tb.Text = shortcutText;

            if (tb.DataContext is HotKeyConfig config)
            {
                config.Shortcut = shortcutText;
            }
        }
    }


    private void ApplyHotkeys()
    {
        InputBindings.Clear();
        CommandBindings.Clear();

        foreach (var config in _vm.HotKeyConfigs)
        {
            if (config.Key == Key.None || !IsValidKeyForGesture(config.Key))
                continue; 

            try
            {
                var gesture = new KeyGesture(config.Key, config.Modifiers);
                var command = new RoutedUICommand(config.Name, config.Type.ToString(), typeof(HotKeyManagment));

                InputBindings.Add(new InputBinding(command, gesture));
            }
            catch (NotSupportedException)
            {
            }
        }
    }

    private bool IsValidKeyForGesture(Key key)
    {
        return key != Key.LeftCtrl &&
               key != Key.RightCtrl &&
               key != Key.LeftAlt &&
               key != Key.RightAlt &&
               key != Key.LeftShift &&
               key != Key.RightShift &&
               key != Key.LWin &&
               key != Key.RWin &&
               key != Key.None;
    }
}
