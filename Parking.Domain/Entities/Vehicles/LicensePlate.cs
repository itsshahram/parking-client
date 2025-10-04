using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Domain.Entities.Vehicles;

public class LicensePlate
{
    [Key]
    public Guid Id { get; set; }
    public string? EnLicensePlate { get; set; }
    public string? FaLicensePlate { get; set; }
    public bool IsLocal { get; set; }
    public Guid? GroupId { get; set; }
    [ForeignKey("GroupId")]
    public LicensePlateGroup? Group { get; set; }
}
