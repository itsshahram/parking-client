using FluentValidation;
using Parking.WebApi.Requests;

namespace Parking.WebApi.Features.Tickets.Commands.UpdateTicketPayment;

public class UpdateTicketPaymentCommandValidator : AbstractValidator<UpdateTicketPaymentCommand>
{
    public UpdateTicketPaymentCommandValidator()
    {
        // بررسی نوع پرداخت
        RuleFor(x => x.PaidType)
            .Must(x => Enum.TryParse<PaidType>(x, true, out _))
            .WithMessage("نوع پرداخت فقط می‌تواند مقدار رشته‌ای Naghdi یا POS باشد");

        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("شناسه تیکت را وارد کنید");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("مبلغ باید بزرگتر از صفر باشد");

        RuleFor(x => x.PaidDate)
            .NotEmpty()
            .WithMessage("تاریخ پرداخت را وارد کنید");

        RuleFor(x => x.ExitGate)
            .NotEmpty()
            .WithMessage("گیت خروج را وارد کنید");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("شناسه دستگاه را وارد کنید");

        // فیلدهایی که برای پرداخت POS اجباری هستند
        RuleFor(x => x.Rrn)
            .NotEmpty()
            .When(x => !IsCashPayment(x.PaidType))
            .WithMessage("مقدار RRN را وارد کنید");

        RuleFor(x => x.MerchantNumber)
            .NotEmpty()
            .When(x => !IsCashPayment(x.PaidType))
            .WithMessage("مقدار MerchantNumber را وارد کنید");

        RuleFor(x => x.PaidCreditCard)
            .NotEmpty()
            .When(x => !IsCashPayment(x.PaidType))
            .WithMessage("مقدار PaidCreditCard را وارد کنید");

        RuleFor(x => x.TraceNo)
            .NotEmpty()
            .When(x => !IsCashPayment(x.PaidType))
            .WithMessage("مقدار TraceNo را وارد کنید");

        RuleFor(x => x.RefId)
            .NotEmpty()
            .When(x => !IsCashPayment(x.PaidType))
            .WithMessage("مقدار RefId را وارد کنید");
    }

    private static bool IsCashPayment(string paidType)
    {
        return string.Equals(paidType, "Naghdi", StringComparison.OrdinalIgnoreCase);
    }
}