

using Parking.App.Models.Dto.Parking.ParkingSection;
using Parking.App.Models.Dto.Parking.ParkingSpace;
using Parking.App.Models.Dto.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class ParkingLotModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public short? Province { get; set; }
    public short? City { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public short FloorsCount { get; set; }
    public TimeOnly StartWorkingHours { get; set; }
    public TimeOnly EndWorkingHours { get; set; }
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public UserListItemModel? Owner { get; set; }
    public Guid CreatorUserId { get; set; }
    public UserListItemModel? CreatorUser { get; set; }
    public List<ParkingSectionModel>? Sections { get; set; }
    public List<ParkingSpaceModel>? ParkingSpaces { get; set; }
}
