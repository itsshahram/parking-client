namespace Parking.App.Models.Dto.Vehicle.LicensePlate;

public class LicensePlatePlateItemDto
{
    public Guid Id { get; set; }
    public string PersianPlate { get; set; } = string.Empty;
    public string EnglishPlate { get; set; } = string.Empty;
}

public class LicensePlateGroupWithPlatesDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public short DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<LicensePlatePlateItemDto> Plates { get; set; } = new();
}

public class LicensePlateGroupWithPlatesPaginatedResult
{
    public int TotalCount { get; set; }
    public int TotalGroups { get; set; }
    public int TotalActiveGroups { get; set; }
    public int TotalInactiveGroups { get; set; }
    public List<LicensePlateGroupWithPlatesDto> Data { get; set; } = new();
}
