namespace Parking.App.Helpers;

using System.ComponentModel;

public static class PermissionBehavior
{
    public static readonly DependencyProperty PermissionProperty =
        DependencyProperty.RegisterAttached(
            "Permission",
            typeof(string),
            typeof(PermissionBehavior),
            new PropertyMetadata(null, OnPermissionChanged));

    private static readonly DependencyProperty HandlerProperty =
        DependencyProperty.RegisterAttached(
            "Handler",
            typeof(PropertyChangedEventHandler),
            typeof(PermissionBehavior),
            new PropertyMetadata(null));

    public static void SetPermission(UIElement element, string value) =>
        element.SetValue(PermissionProperty, value);

    public static string GetPermission(UIElement element) =>
        (string)element.GetValue(PermissionProperty);

    private static void OnPermissionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        if (GetHandler(element) is PropertyChangedEventHandler oldHandler)
        {
            PermissionManager.Instance.PropertyChanged -= oldHandler;
            SetHandler(element, null);
        }

        if (e.NewValue is string permission && !string.IsNullOrEmpty(permission))
        {
            PropertyChangedEventHandler handler = (s, args) =>
            {
                if (args.PropertyName == nameof(PermissionManager.UserPermissions))
                    element.Dispatcher.Invoke(() => UpdateElementVisibility(element, permission));
            };

            if (element is FrameworkElement fe)
                fe.Unloaded += (_, __) => PermissionManager.Instance.PropertyChanged -= handler;

            PermissionManager.Instance.PropertyChanged += handler;
            SetHandler(element, handler);

            element.Dispatcher.Invoke(() => UpdateElementVisibility(element, permission));
        }
    }

    private static void UpdateElementVisibility(UIElement element, string permission)
    {
        if (element == null || string.IsNullOrEmpty(permission)) return;

        var hasPermission = PermissionManager.Instance.HasPermission(permission);
        element.Visibility = hasPermission ? Visibility.Visible : Visibility.Collapsed;
    }

    private static PropertyChangedEventHandler? GetHandler(DependencyObject obj) =>
        (PropertyChangedEventHandler?)obj.GetValue(HandlerProperty);

    private static void SetHandler(DependencyObject obj, PropertyChangedEventHandler? handler) =>
        obj.SetValue(HandlerProperty, handler);
}
