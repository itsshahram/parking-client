
namespace Parking.App.Models.Login;

public class LoginResponse
{
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public int? ParkingLotId { get; set; }
    public Guid UserId { get; set; }
}