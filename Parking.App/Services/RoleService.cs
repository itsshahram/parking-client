using Parking.Domain.Entities;
using Parking.Domain.Entities.User;

namespace Parking.App.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleService(IUnitOfWork unitOfWork, RoleManager<ApplicationRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissions(Guid RoleId)
        => await _unitOfWork.RolePermissions.GetAll()
                                            .Where(rp => rp.ApplicationRole != null && rp.ApplicationRole.Id == RoleId)
                                            .ToListAsync();


    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        => await _unitOfWork.Permissions.GetAll().ToListAsync();

    public async Task<IEnumerable<ApplicationRole>?> GetRoles()
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
}

