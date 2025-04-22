using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Parking.App.Models;

public class ReceiptModel
{
    public string? Description { get; set; }
    public string? LicensePlate { get; set; }
    public long BarcodeId { get; set; }
    public string? VehicleSegmentName { get; set; }
    public string? ParkingName { get; set; }
    public string? StartTime { get; set; }
    public BitmapImage? BarcodeImage { get; set; }
}
public class InvoiceModel : ReceiptModel
{
    public string? TotalAmount { get; set; }
    public string? TotalDiscount { get; set; }
    public string? PaidAmount { get; set; }
    public string? EndTime { get; set; }
}
