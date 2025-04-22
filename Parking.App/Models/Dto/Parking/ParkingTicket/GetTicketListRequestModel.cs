using Parking.Domain.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class GetTicketListRequestModel
{
    public string? LicensePlate { get; set; }
    public int? ParkingId { get; set; }
    public Guid? SectionId { get; set; }
    public long? BarcodeId { get; set; }
    public int? VehicleSegmentId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? VehicleOwnerId { get; set; }
    public Guid? ParkingSpaceID { get; set; }
    public string? PaidType { get; set; }
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
    public bool? IsExited { get; set; }
    public bool? IsPaid { get; set; }
    public DateTime? StartStartTime { get; set; }
    public DateTime? EndStartTime { get; set; }
    public TicketStatus? TicketStatus { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
