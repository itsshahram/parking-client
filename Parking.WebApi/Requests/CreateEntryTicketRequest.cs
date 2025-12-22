using System.ComponentModel.DataAnnotations;

namespace Parking.WebApi.Requests;

public class CreateEntryTicketRequest
{
    [Required(ErrorMessage = "پلاک الزامی است")]
    public string EnLicensePlate { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع پلاک را مشخص کنید")]
    public PlateType PlateType { get; set; }

    [Required(ErrorMessage = "شناسه تعرفه را وارد کنید")]
    public int VehicleSegmentId { get; set; }

    public string DeviceName { get; set; } = string.Empty;
    public List<string>? Base64Images { get; set; }
    public long? CardUid { get; set; }
}