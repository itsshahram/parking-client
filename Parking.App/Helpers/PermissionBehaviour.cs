namespace Parking.App.Helpers;

public static class PermissionBehavior
{
    public static readonly DependencyProperty PermissionProperty =
        DependencyProperty.RegisterAttached(
            "Permission",
            typeof(string),
            typeof(PermissionBehavior),
            new PropertyMetadata(null, OnPermissionChanged));

    public static void SetPermission(UIElement element, string value) =>
        element.SetValue(PermissionProperty, value);

    public static string GetPermission(UIElement element) =>
        (string)element.GetValue(PermissionProperty);

    private static void OnPermissionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && e.NewValue is string permission)
        {
            UpdateElementVisibility(element, permission);

            PermissionManager.Instance.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(PermissionManager.UserPermissions))
                    UpdateElementVisibility(element, permission);
            };
        }
    }

    private static void UpdateElementVisibility(UIElement element, string permission)
    {
        var hasPermission = PermissionManager.Instance.HasPermission(permission);
        element.Visibility = hasPermission ? Visibility.Visible : Visibility.Collapsed;
    }
}