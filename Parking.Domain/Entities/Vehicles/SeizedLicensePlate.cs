using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Vehicles;

public class SeizedLicensePlate
{
    [Key]
    public Guid Id { get; set; }
    public string? EnLicensePlate { get; set; }
    public string? FaLicensePlate { get; set; }
    public string? SeizedReason { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid? CreatorUserId { get; set; }
}
