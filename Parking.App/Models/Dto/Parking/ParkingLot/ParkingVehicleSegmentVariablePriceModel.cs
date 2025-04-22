using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class ParkingVehicleSegmentVariablePriceModel
{
    public long Id { get; set; }
    /// <summary>
    /// آی دی پارکینگ
    /// </summary>
    public int ParkingLotId { get; set; }
    /// <summary>
    /// آی دی سگنت
    /// </summary>
    public int VehicleSegmentId { get; set; }
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
