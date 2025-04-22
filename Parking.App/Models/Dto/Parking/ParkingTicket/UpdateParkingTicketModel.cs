
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class UpdateParkingTicketModel
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid VehicleOwnerId { get; set; }
    public Guid ParkingSpaceID { get; set; }
    public int ParkingId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsExited { get; set; }
    public bool IsPaid { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaidCreditCard { get; set; }
    public string? EntranceGate { get; set; }
    public string? ExitGate { get; set; }

    [MaxLength(150)]
    public string? PaidType { get; set; }
    public string? RefId { get; set; }
    //ExtraInfo
    public string? DriverFullName { get; set; }
    public string? DriverPhoneNumber { get; set; }
    public string? DriverDescription { get; set; }
}
