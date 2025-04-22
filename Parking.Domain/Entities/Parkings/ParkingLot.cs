using Parking.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Parkings;

public class ParkingLot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
    public Double? latitude { get; set; }
    public Double? longitude { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid CreatorUserId { get; set; }
}
