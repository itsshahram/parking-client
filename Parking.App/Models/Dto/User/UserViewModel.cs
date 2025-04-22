

namespace Parking.App.Models.Dto.Users;

public class UserViewModel
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public DateTime RegisterDate { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }

    public bool PhoneNumberConfirmed { get; set; }
}
