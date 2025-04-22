using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Card;

public class CardModel
{
    public int Id { get; set; }

    public long PersonnelId { get; set; }

    public string? OwnerFirstName { get; set; }

    public string? OwnerLastName { get; set; }
    public string? OwnerNationalCode { get; set; }
    public string? OwnerAddress { get; set; }
    public string? EnLicensePlate { get; set; }

    public DateTime ActiveDate { get; set; }

    public DateTime DeactiveDate { get; set; }

    public int FixDiscount { get; set; }

    public int PercentDiscount { get; set; }

    public Guid? LicensePlateGroupId { get; set; }
    public int? VehicleSegmentId { get; set; }

    public long? Credit { get; set; }

    public decimal? CardSerialNo { get; set; }

    public bool IsGuest { get; set; }
    public bool IsActive { get; set; }
    public bool IsInUse { get; set; }

    public byte[]? OwnerPic { get; set; }

}
