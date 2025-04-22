using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Parkings;

public class Card
{
    [Key]
    public int Id { get; set; }


    public string? OwnerFirstName { get; set; }

    public string? OwnerLastName { get; set; }
    public string? OwnerNationalCode { get; set; }
    public string? OwnerAddress { get; set; }

    public DateTime ActiveDate { get; set; }

    public DateTime DeactiveDate { get; set; }

    public int FixDiscount { get; set; }

    public int PercentDiscount { get; set; }
    public string? EnLicensePlate { get; set; }

    public Guid? LicensePlateGroupId { get; set; }
    public int? VehicleSegmentId { get; set; }

    public long? Credit { get; set; }

    public decimal? CardSerialNo { get; set; }

    public bool IsGuest { get; set; }
    public bool IsActive { get; set; }
    public bool IsInUse { get; set; }

    public byte[]? OwnerPic { get; set; }

}
