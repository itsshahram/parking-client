

using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.Entities.User;

public class UserAction
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid? UserId { get; set; }
    public int? ParkingLotId { get; set; }
    public Guid? ParkingTicketId { get; set; }
    public string? Description { get; set; }
}
