
using Parking.Domain.Entities.Vehicles;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Domain.Entities.Parkings;

public class ParkingVehicleSegmentPrice
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
    public bool IsVariableEnable { get; set; }

}

