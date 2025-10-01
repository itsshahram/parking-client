using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.General;

public enum LicensePlateGroupDate
{
    [Display(Name = "یک ماه")]
    OneMonth,
    [Display(Name = "سه ماه")]
    ThreeMonth,
    [Display(Name = "شش ماه")]
    SixMonth,
    [Display(Name = "دلخواه")]
    Optional
}
