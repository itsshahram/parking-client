using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Vehicle.VehicleSegment;

public class ParkingVehicleSegmentPriceModel
{
    public int Id { get; set; }
    public int ParkingLotId { get; set; }
    public int VehicleSegmentId { get; set; }

    /// <summary>
    /// مبلغ ساعتی.
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// ساعت از
    /// </summary>
    public TimeOnly TimeFrom { get; set; }
    /// <summary>
    /// ساعت تا
    /// </summary>
    public TimeOnly TimeTo { get; set; }
    /// <summary>
    /// فعال بودن متغیر
    /// </summary>

    public bool IsVariableEnable { get; set; }
}
