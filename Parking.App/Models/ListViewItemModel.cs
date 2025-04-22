using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models;

public class ListViewItemModel
{
    public string? Title { get; set; }
    public string? PublishTime { get; set; }
    public string? Description { get; set; }
}

