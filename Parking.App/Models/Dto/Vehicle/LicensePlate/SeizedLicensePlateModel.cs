using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Vehicle.LicensePlate;

public class SeizedLicensePlateModel
{
    public Guid Id { get; set; }
    public string? EnLicensePlate { get; set; }
    public string? FaLicensePlate { get; set; }
    public string? SeizedReason { get; set; }
    public DateTime CreateDate { get; set; }
    public string? CreateDateShamsi { get; set; }

    public Guid? CreatorUserId { get; set; }
}
