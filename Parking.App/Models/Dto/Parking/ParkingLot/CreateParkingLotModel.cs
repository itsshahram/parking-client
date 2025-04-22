using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class CreateParkingLotModel
{
    [DisplayName("نام کاربری")]
    [Required(ErrorMessage = "ورود {0} الزامی است")]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public IFormFile?  ImageFile { get; set; }
    [DisplayName("استان")]
    [Required(ErrorMessage = "ورود {0} الزامی است")]
    public short Province { get; set; }
    [DisplayName("شهر")]
    [Required(ErrorMessage = "ورود {0} الزامی است")]
    public short City { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public short FloorsCount { get; set; }
    public TimeOnly StartWorkingHours { get; set; }
    public TimeOnly EndWorkingHours { get; set; }
    public bool IsActive { get; set; }
    public short ParkingMinimumTime { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public Double latitude { get; set; }
    public Double longitude { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid CreatorUserId { get; set; }

}
