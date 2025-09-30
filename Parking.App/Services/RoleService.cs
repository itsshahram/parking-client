using Parking.Domain.Entities;
using Parking.Domain.Entities.User;

namespace Parking.App.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleService(IUnitOfWork unitOfWork, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissions(Guid RoleId)
        => await _unitOfWork.RolePermissions.GetAll()
                                            .Include(rp => rp.ApplicationRole)
                                            .Include(rp => rp.Permission)
                                            .Where(rp => rp.ApplicationRole != null && rp.ApplicationRole.Id == RoleId)
                                            .ToListAsync();


    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        => await _unitOfWork.Permissions.GetAll().ToListAsync();

    public async Task<List<ApplicationRole>> GetRoles()
        => await _unitOfWork.Roles.ToListAsync();

    public async Task<ApplicationRole?> GetRoleByName(string Name)
        => await _roleManager.FindByNameAsync(Name);
    public async Task<(bool IsSuccess, bool IsExist)> Create(ApplicationRole role)
    {
        var existingRole = await _roleManager.FindByNameAsync(role.Name);
        if (existingRole != null)
            return (false, true);

        var result = await _roleManager.CreateAsync(role);
        return (result.Succeeded, false);
    }

    public async Task<bool> AddToRolePermission(Guid RoleId, IEnumerable<Guid> Ids)
    {

        await _unitOfWork.RolePermissions.ExecuteDeleteAsync(rp => rp.ApplicationRoleId == RoleId);

        foreach (var permissionId in Ids)
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission()
            {
                ApplicationRoleId = RoleId,
                PermissionId = permissionId,
                CreateDate = DateTime.Now,
            });
        }

        return true;
    }

    public Task<bool> DeleteRole(Guid roleId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<UserPermission>> GetUserPermissions(Guid RoleId, Guid UserId)
        => await _unitOfWork.UserPermissions.GetAll()
        .Include(rp => rp.ApplicationRole)
        .Include(rp => rp.ApplicationUser)
        .Include(rp=>rp.Permission)
        .Where(rp => rp.ApplicationRole != null && rp.ApplicationRole.Id == RoleId && rp.ApplicationUser.Id == UserId)
        .ToListAsync();

    public async Task<bool> AddToUserPermissions(Guid RoleId, Guid UserId, IEnumerable<Guid> Ids)
    {
        await _unitOfWork.UserPermissions.ExecuteDeleteAsync(rp => rp.ApplicationRoleId == RoleId);
        foreach (var permissionId in Ids)
        {
            await _unitOfWork.UserPermissions.AddAsync(new UserPermission()
            {
                ApplicationRoleId = RoleId,
                ApplicationUserId = UserId,
                PermissionId = permissionId,
            });
        }
        return true;
    }

    public async Task<bool> AssignRoleToUser(Guid userId, Guid roleId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return false;
        }

        var addResult = await _userManager.AddToRoleAsync(user, role.Name);
        return addResult.Succeeded;
    }
}

