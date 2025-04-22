using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Config;

public class CameraConfigModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? RTSPUrl { get; set; }
    public string? SnapshotUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
}
