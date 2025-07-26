namespace Parking.App.Models.Dto.User;

public class ParkingUserViewModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? ParkingLoId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public string? PasswordHash { get; set; }
    public string? Role { get; set; }
    public string? PhoneNumber { get; internal set; }
}
