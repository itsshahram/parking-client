

namespace Parking.Domain.Entities.Parkings;

public class AddCardItem
{
    public Guid Id { get; set; }
    public long? CardUid { get; set; }
    public string? OwnerFullName { get; set; }
    public DateTime ActiveDate { get; set; }
    public DateTime DeactiveDate { get; set; }
    public string? Description { get; set; }
    public string? EnLicensePlate { get; set; }
    public int PercentDiscount { get; set; }
    public int? VehicleSegmentId { get; set; }
    public DateTime CreateDate { get; set; }
}
