using Parking.Domain.Entities;
using Parking.Domain.Entities.User;

namespace Parking.App.Seeds;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(DbContextOptions<ApplicationDbContext> options,
                                                   RoleManager<ApplicationRole> roleManager)
    {
        var discoveredPermissions = PermissionScanner.GetAllPermissions();

        using var context = new ApplicationDbContext(options);

        var existingNames = await context.Permissions
                                         .Select(p => p.Name)
                                         .ToListAsync();

        var newPermissions = discoveredPermissions
            .Where(p => !existingNames.Contains(p.Name))
            .ToList();

        if (newPermissions.Any())
        {
            await context.Permissions.AddRangeAsync(newPermissions);
            await context.SaveChangesAsync();
        }

        var parkingManagerRole = await roleManager.FindByNameAsync("ParkingManager");
        if (parkingManagerRole == null)
        {
            parkingManagerRole = new ApplicationRole
            {
                Name = "ParkingManager",
                FaName = "مدیر پارکینگ"
            };
            await roleManager.CreateAsync(parkingManagerRole);
        }

        var hasAnyRolePermissions = await context.RolePermissions
            .AnyAsync(rp => rp.ApplicationRoleId == parkingManagerRole.Id);

        if (!hasAnyRolePermissions)
        {
            var allPermissions = await context.Permissions.ToListAsync();
            var rolePermissions = allPermissions.Select(p => new RolePermission
            {
                ApplicationRoleId = parkingManagerRole.Id,
                PermissionId = p.Id,
                CreateDate = DateTime.Now
            }).ToList();

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
