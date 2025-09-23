using Microsoft.AspNetCore.Identity;

namespace Parking.Domain.Entities.User;
public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public int? ParkingLotId { get; set; }
    public string? Avatar { get; set; }
    public DateTime RegisterDate { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

