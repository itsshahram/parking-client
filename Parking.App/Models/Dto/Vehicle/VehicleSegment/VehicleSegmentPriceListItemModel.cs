using Parking.App.Models.Dto.Parking.ParkingLot;
using Parking.Domain.Entities.Parkings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Vehicle.VehicleSegment;

public class VehicleSegmentPriceListItemModel
{
    public int Id { get; set; }
    public string? NameFa { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int ParkingLotId { get; set; }
    /// <summary>
    /// مبلغ ثابت ورودی پارکینگ.
    /// </summary>
    public decimal ParkingEntranceFixedFee { get; set; }
    /// <summary>
    /// مبلغ روزانه.
    /// </summary>
    public decimal DailyRate { get; set; }
    /// <summary>
    /// میزان دقیقه رایگان ورودی.
    /// </summary>
    public int FreeEntranceMinutes { get; set; }
    public List<ParkingVehicleSegmentPriceListItemModel>? VehicleSegmentPrices { get; set; }
}

public class ParkingVehicleSegmentPriceListItemModel   : ParkingVehicleSegmentPriceModel
{
    public List<ParkingVehicleSegmentVariablePriceModel>? VehicleSegmentVariablePrices { get; set; }
}