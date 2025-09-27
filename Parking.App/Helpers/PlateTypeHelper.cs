using Parking.Domain.General;

namespace Parking.App.Helpers;

public static class PlateTypeHelper
{
    /// <summary>
    /// دریافت همه مقادیر PlateType به صورت لیست
    /// </summary>
    public static List<PlateType> GetAllPlateType()
    {
        return Enum.GetValues(typeof(PlateType)).Cast<PlateType>().ToList();
    }

    /// <summary>
    /// تبدیل نوع پلاک به معادل فارسی
    /// </summary>
    public static string ToPersian(PlateType plateType)
    {
        return plateType switch
        {
            PlateType.All => "همه",
            PlateType.IranianPlate => "پلاک های ایرانی",
            PlateType.Other => "سایر",
            _ => "نامشخص"
        };
    }

    /// <summary>
    /// تبدیل نوع پلاک به معادل انگلیسی قابل نمایش
    /// </summary>
    public static string ToEnglish(PlateType plateType)
    {
        return plateType switch
        {
            PlateType.All => "All",
            PlateType.IranianPlate => "Iranian Plate",
            PlateType.Other => "Other",
            _ => "Unknown"
        };
    }

}