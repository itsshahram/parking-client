using Microsoft.AspNetCore.Http;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class UpdateParkingLotModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public IFormFile? ImageFile { get; set; }
    public short? Province { get; set; }
    public short? City { get; set; }
    public string? Address { get; set; }
    public int? Capacity { get; set; }
    public short? FloorsCount { get; set; }
    public TimeOnly? StartWorkingHours { get; set; }
    public TimeOnly? EndWorkingHours { get; set; }
    public bool? IsActive { get; set; }
    public short? ParkingMinimumTime { get; set; }
    public bool? IsOnline { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public Double? latitude { get; set; }
    public Double? longitude { get; set; }
}
