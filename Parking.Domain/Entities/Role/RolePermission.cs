using Parking.Domain.Entities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Domain.Entities;

public class RolePermission
{
    [Key]
    public Guid Id { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;

    public Guid ApplicationRoleId { get; set; }
    [ForeignKey(nameof(ApplicationRoleId))]
    public ApplicationRole ApplicationRole { get; set; }

    public Guid PermissionId { get; set; }
    [ForeignKey(nameof(PermissionId))]
    public Permission Permission { get; set; }

}
