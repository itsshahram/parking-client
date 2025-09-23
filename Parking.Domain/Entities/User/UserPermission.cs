using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Domain.Entities.User
{
    public class UserPermission
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ApplicationRoleId { get; set; }
        [ForeignKey(nameof(ApplicationRoleId))]
        public ApplicationRole ApplicationRole { get; set; }

        public Guid ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser  { get; set; }

        public Guid PermissionId { get; set; }
        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; }
    }
}
