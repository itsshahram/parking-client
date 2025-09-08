namespace Parking.App.Views.Components;
using System.Windows;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;

public class ShortcutTextBox : TextBox
{
    public static readonly DependencyProperty ShortcutProperty =
        DependencyProperty.Register(nameof(Shortcut), typeof(string), typeof(ShortcutTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Shortcut
    {
        get => (string)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public ShortcutTextBox()
    {
        IsReadOnly = true;
        MinWidth = 200;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        e.Handled = true;

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.None)
            key = e.ImeProcessedKey;

        ModifierKeys modifiers = Keyboard.Modifiers;

        // Format display text:
        Shortcut = modifiers == ModifierKeys.None
            ? $"{key}"
            : $"{modifiers} + {key}";

        Text = Shortcut;
    }
}
