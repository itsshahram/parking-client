using System.Text.RegularExpressions;
using FluentValidation;
using Parking.WebApi.Requests;

namespace Parking.WebApi.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    // public CreateTicketCommandValidator()
    // {
    //     RuleFor(x => x.EnLicensePlate)
    //         .NotEmpty().WithMessage("پلاک الزامی است")
    //         .MaximumLength(50).WithMessage("پلاک نمی‌تواند بیشتر از 50 کاراکتر باشد");
    //
    //     RuleFor(x => x.PlateType)
    //         .IsInEnum().WithMessage("نوع پلاک نامعتبر است");
    //
    //     RuleFor(x => x.VehicleSegmentId)
    //         .GreaterThan(0).WithMessage("شناسه تعرفه باید بزرگتر از صفر باشد");
    //
    //     RuleFor(x => x.DeviceName)
    //         .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
    // }
    private static readonly Regex NormalPlateRegex = new Regex(
        @"^\d{2,3}_[a-z]+_\d{3}_IR\d{2}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ValidMiddleParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "a", "b", "p", "t", "se", "gh", "f", "g", "k", "l", "de", "m", "n", "v",
        "zh", "sin", "sh", "sad", "h", "ta", "za", "ain", "y", "jim", "alef", "z"
    };
    
    private static readonly string ValidMiddlePartsMessage = string.Join(", ", ValidMiddleParts.OrderBy(x => x));

    private static readonly Regex MotorcycleRegex = new Regex(
        @"^\d{8}$",
        RegexOptions.Compiled);

    private static readonly Regex FreeZoneRegex = new Regex(
        @"^\d{5}_\d{2}$",
        RegexOptions.Compiled);

    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.EnLicensePlate)
            .NotEmpty().WithMessage("پلاک الزامی است")
            .MaximumLength(50).WithMessage("پلاک نمی‌تواند بیشتر از 20 کاراکتر باشد");

        RuleFor(x => x.PlateType)
            .IsInEnum().WithMessage("نوع پلاک نامعتبر است");

        RuleFor(x => x.VehicleSegmentId)
            .GreaterThan(0).WithMessage("شناسه تعرفه باید بزرگتر از صفر باشد");

        RuleFor(x => x.DeviceName)
            .MaximumLength(100).When(x => x.DeviceName != null)
            .WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

        // ولیدیشن شرطی بر اساس PlateType
        RuleFor(x => x.EnLicensePlate)
            //.Must(BeValidNormalPlate)
            .Matches(NormalPlateRegex)
            .When(x => x.PlateType == PlateType.NormalPersianPlates)
            .WithMessage("پلاک خودرو نامعتبر است");

        RuleFor(x => x.EnLicensePlate)
            .Must(HaveValidMiddlePart)
            //.When(x => x.PlateType == PlateType.NormalPersianPlates)
            .When(x => x.PlateType == PlateType.NormalPersianPlates && NormalPlateRegex.IsMatch(x.EnLicensePlate ?? ""))
            .WithMessage("حرف وسط پلاک نامعتبر است. Valid parts: " + ValidMiddlePartsMessage);

        RuleFor(x => x.EnLicensePlate)
            .Matches(MotorcycleRegex)
            .When(x => x.PlateType == PlateType.Motorcycle)
            .WithMessage("پلاک موتورسیکلت باید دقیقاً 8 رقم عددی باشد");

        RuleFor(x => x.EnLicensePlate)
            .Matches(FreeZoneRegex)
            .When(x => x.PlateType == PlateType.FreeZone)
            .WithMessage("فرمت پلاک منطقه آزاد نامعتبر است");
    }

    private bool BeValidNormalPlate(string plate)
    {
        if (string.IsNullOrEmpty(plate))
            return false;

        var match = NormalPlateRegex.Match(plate.Trim());
        if (!match.Success)
            return false;

        var parts = plate.Split('_');
        if (parts.Length != 4)
            return false;

        var middlePart = parts[1].Trim();

        return ValidMiddleParts.Contains(middlePart);
    }
    
    private static bool HaveValidMiddlePart(string plate)
    {
        if (string.IsNullOrEmpty(plate)) return false;

        var parts = plate.Trim().Split('_');
        if (parts.Length != 4) return true; // اگر ساختار اشتباه باشه، قانون قبلی خطا می‌ده

        var middlePart = parts[1].Trim();
        return ValidMiddleParts.Contains(middlePart);
    }
}
