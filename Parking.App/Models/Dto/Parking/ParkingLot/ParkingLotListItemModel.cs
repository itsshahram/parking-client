

using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class ParkingLotListItemModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public short? Province { get; set; }
    public short? City { get; set; }
    public int Capacity { get; set; }
    public short FloorsCount { get; set; }
    public TimeOnly StartWorkingHours { get; set; }
    public TimeOnly EndWorkingHours { get; set; }
    public bool IsActive { get; set; }
    public short ParkingMinimumTime { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid CreatorUserId { get; set; }
}
