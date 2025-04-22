

namespace Parking.App.Models.Dto.Users;

public class CreateUserModel
{

    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? PhoneNumber { get; set; }
    public required string Password { get; set; }
    public string? UserName { get; set; }
    public required string Email { get; set; }
}
