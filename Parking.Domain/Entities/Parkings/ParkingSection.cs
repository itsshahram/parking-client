using Parking.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.Parkings;

public class ParkingSection
{
    [Key]
    public Guid Id { get; set; }
    public short SectionNumber { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IndexName { get; set; }
    public short? Floor { get; set; }
    public int Capacity { get; set; }
    public DateTime CreateDate { get; set; }
    public int ParkingId { get; set; }
}
