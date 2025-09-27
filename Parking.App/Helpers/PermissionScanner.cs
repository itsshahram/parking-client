namespace Parking.App.Helpers;

using Parking.Domain.Entities;
using System.Reflection;
using System.Diagnostics;
using Parking.App.Attributes;

public static class PermissionScanner
{
    public static List<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a =>
                !a.IsDynamic &&
                (a.FullName?.StartsWith("Parking.App") == true ||
                 a.FullName?.StartsWith("Parking.Domain") == true));

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();

                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Debug.WriteLine(loaderEx.Message);
                }
            }

            foreach (var type in types)
            {
                if (type == null) continue;

                // Class-level attributes
                foreach (var attr in type.GetCustomAttributes<RequiresPermissionAttribute>(true))
                {
                    permissions.Add(new Permission
                    {
                        Name = attr.Permission,
                        FaName = attr.PermissionFaName
                    });
                }

                // Method-level attributes
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    foreach (var attr in method.GetCustomAttributes<RequiresPermissionAttribute>(true))
                    {
                        permissions.Add(new Permission
                        {
                            Name = attr.Permission,
                            FaName = attr.PermissionFaName
                        });
                    }
                }
            }
        }

        // Remove duplicates (by Name)
        return permissions
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .ToList();
    }
}

