using FluentValidation;

namespace Parking.WebApi.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.EnLicensePlate)
            .NotEmpty().WithMessage("پلاک الزامی است")
            .MaximumLength(50).WithMessage("پلاک نمی‌تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.PlateType)
            .IsInEnum().WithMessage("نوع پلاک نامعتبر است");

        RuleFor(x => x.VehicleSegmentId)
            .GreaterThan(0).WithMessage("شناسه تعرفه باید بزرگتر از صفر باشد");

        RuleFor(x => x.DeviceName)
            .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
    }
}
