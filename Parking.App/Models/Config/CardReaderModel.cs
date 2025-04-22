using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Config;

public class CardReaderModel
{
    public string? Name { get; set; }
    public int Id { get; set; }
    public string? COMNo { get; set; }
    public int? COMBPS { get; set; }
}
