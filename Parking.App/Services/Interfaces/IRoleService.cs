using Parking.Domain.Entities;
using Parking.Domain.Entities.User;

namespace Parking.App.Services.Interfaces;

public interface IRoleService
{

    Task<ApplicationRole?> GetRoleByName(string Name);
    Task<IEnumerable<Permission>> GetPermissionsAsync();
    Task<IEnumerable<ApplicationRole>?> GetRoles();
    Task<IEnumerable<RolePermission>> GetRolePermissions(Guid RoleId);
    Task<(bool IsSuccess, bool IsExist)> Create(ApplicationRole role);
}
