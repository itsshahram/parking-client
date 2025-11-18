using Parking.Domain.Entities.Vehicles;
using Parking.Domain.General;
using System.Globalization;

namespace Parking.App.ViewModels.Windows;

public class EditLicensePlateGroupViewModel : AddLicensePlateGroupViewModel
{
    private readonly PersianCalendar _pc = new PersianCalendar();

    public Guid Id { get; }

    public EditLicensePlateGroupViewModel(LicensePlateGroup group)
    {
        Id = group.Id;

        Name = group.Name;
        Description = group.Description;
        DiscountPercent = group.DiscountPercent;

        var start = group.StartDate;
        StartDate = start;
        StartYear = _pc.GetYear(start).ToString();
        StartMonth = _pc.GetMonth(start).ToString("00");
        StartDay = _pc.GetDayOfMonth(start).ToString("00");

        var end = group.EndDate;
        EndDate = end;
        EndYear = _pc.GetYear(end).ToString();
        EndMonth = _pc.GetMonth(end).ToString("00");
        EndDay = _pc.GetDayOfMonth(end).ToString("00");
        EndDateString = end.ToShamsi();

        var monthDiff = ((end.Year - start.Year) * 12) + (end.Month - start.Month);
        GroupDate = monthDiff switch
        {
            1 => LicensePlateGroupDate.OneMonth,
            3 => LicensePlateGroupDate.ThreeMonth,
            6 => LicensePlateGroupDate.SixMonth,
            _ => LicensePlateGroupDate.Optional
        };

        IsOptionalSelected = GroupDate == LicensePlateGroupDate.Optional;
    }
}