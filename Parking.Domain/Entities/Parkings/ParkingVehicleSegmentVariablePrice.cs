using Parking.Domain.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Parkings;

public class ParkingVehicleSegmentVariablePrice
{
    [Key]
    public long Id { get; set; }
    public int ParkingLotId { get; set; }
    public int VehicleSegmentId { get; set; }
    [ForeignKey("ParkingLotId")]
    public ParkingLot? ParkingLot { get; set; }
    [ForeignKey("VehicleSegmentId")]
    public VehicleSegment? VehicleSegment { get; set; }
    /// <summary>
    /// شماره
    /// </summary>
    public int Number { get; set; }
    /// <summary>
    /// دقیقه
    /// </summary>
    public int Minutes { get; set; }
    /// <summary>
    /// قیمت
    /// </summary>
    public decimal? Price { get; set; }
}
