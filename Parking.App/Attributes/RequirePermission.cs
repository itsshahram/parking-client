namespace Parking.App.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequiresPermissionAttribute : Attribute
{
    public string Permission { get; }
    public string PermissionFaName { get; set; }

    public RequiresPermissionAttribute(string permission, string faPermission)
    {
        Permission = permission;
        PermissionFaName = faPermission;
    }
}
