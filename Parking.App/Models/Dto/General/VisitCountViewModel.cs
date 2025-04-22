using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.General;

public class VisitCountViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? FaDate { get; set; }
    public int Count { get; set; }
    public int UniqueVisitorCount { get; set; }
}
