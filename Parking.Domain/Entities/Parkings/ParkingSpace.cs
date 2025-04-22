using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Parkings;

public class ParkingSpace
{
    [Key]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SpaceNumber { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDate { get; set; }
    public int ParkingId { get; set; }
    public Guid ParkingSectionId { get; set; }
    //public Guid CreatorUserId { get; set; }

}
