using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Config;

public class POSConfigModel
{
    public string? Name { get; set; }
    public string? IpAddress { get; set; }
    public int Port { get; set; }
}
