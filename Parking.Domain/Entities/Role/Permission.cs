using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.Entities;

public class Permission
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FaName { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
