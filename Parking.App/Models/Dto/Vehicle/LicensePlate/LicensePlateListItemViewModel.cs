namespace Parking.App.Models.Dto.Vehicle.LicensePlate;

public class LicensePlateListItemViewModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? FaLicensePlate { get; set; }
    public short DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public string? StartDateString { get; set; }
    public DateTime EndDate { get; set; }
    public string? EndDateString { get; set; }
}

public class LicensePlateListItemResult
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public short DiscountPercent { get; set; }
    public string? StartDateString { get; set; }
    public string? EndDateString { get; set; }
}
