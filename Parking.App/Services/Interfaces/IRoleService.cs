using Parking.Domain.Entities;
using Parking.Domain.Entities.User;

namespace Parking.App.Services.Interfaces;

public interface IRoleService
{
    Task<ApplicationRole?> GetRoleByName(string Name);
    Task<IEnumerable<Permission>> GetPermissionsAsync();
    Task<List<ApplicationRole>> GetRoles();
    Task<IEnumerable<RolePermission>> GetRolePermissions(Guid RoleId);
    Task<IEnumerable<UserPermission>> GetUserPermissions(Guid RoleId, Guid UserId);
    Task<bool> AssignRoleToUser(Guid UserId, Guid RoleId);
    Task<bool> AddToUserPermissions(Guid RoleId, Guid UserId, IEnumerable<Guid> Ids);
    Task<(bool IsSuccess, bool IsExist)> Create(ApplicationRole role);
    Task<bool> AddToRolePermission(Guid RoleId, IEnumerable<Guid> Ids);
    Task<bool> DeleteRole(Guid roleId);
}
