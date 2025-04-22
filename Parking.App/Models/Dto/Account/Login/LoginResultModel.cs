

namespace Parking.App.Models.Dto.Account.Login;

public class LoginResultModel
{
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public int? ParkingLotId { get; set; }
    public Guid UserId { get; set; }
}
