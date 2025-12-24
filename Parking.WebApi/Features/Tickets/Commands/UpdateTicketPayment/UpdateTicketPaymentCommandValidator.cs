using FluentValidation;
using Parking.WebApi.Requests;

namespace Parking.WebApi.Features.Tickets.Commands.UpdateTicketPayment;

public class UpdateTicketPaymentCommandValidator : AbstractValidator<UpdateTicketPaymentCommand>
{
    public UpdateTicketPaymentCommandValidator()
    {
        RuleFor(x => x.PaidType)
            .Must(x => Enum.TryParse<PaidType>(x, true, out _))
            .WithMessage("نوع مقدار فقط می تواند مقدار رشته ای Naghdi یا POS باشد");

        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("آی دی تیکت را وارد کنید");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("مبلغ باید بزرگتر از صفر باشد");

        RuleFor(x => x.PaidDate)
            .NotEmpty()
            .WithMessage("تاریخ پرداخت را وارد کنید");

        RuleFor(x => x.ExitGate)
            .NotEmpty()
            .WithMessage("گیت خروج را وارد کنید");
        
        RuleFor(x => x.Rrn)
            .NotEmpty()
            .WithMessage("RRN را وارد کنید");
        
        RuleFor(x => x.MerchantNumber)
            .NotEmpty()
            .WithMessage("شماره پذیرنده را وارد کنید");
        
        RuleFor(x => x.PaidCreditCard)
            .NotEmpty()
            .WithMessage("مقدار PaidCreditCard را وارد کنید");
        
        RuleFor(x => x.TraceNo)
            .NotEmpty()
            .WithMessage("TraceNo را وارد کنید");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("شناسه دستگاه را وارد کنید");
        
        RuleFor(x => x.RefId)
            .NotEmpty()
            .WithMessage("شناسه مرجع را وارد کنید");
    }
}