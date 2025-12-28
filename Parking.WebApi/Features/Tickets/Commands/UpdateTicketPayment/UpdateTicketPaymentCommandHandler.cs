using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Requests;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Commands.UpdateTicketPayment;

public class UpdateTicketPaymentCommandHandler(
    ITicketsService ticketsService) : IRequestHandler<UpdateTicketPaymentCommand, Result>
{
    public async Task<Result> Handle(UpdateTicketPaymentCommand updateCommand, CancellationToken cancellationToken)
    {
        try
        {
            var request = new PaymentRequest
            {
                PaidType = updateCommand.PaidType,
                Base64Images = updateCommand.Base64Images,
                TicketId = updateCommand.TicketId,
                Amount = updateCommand.Amount,
                ExitGate = updateCommand.ExitGate,
                DeviceId = updateCommand.DeviceId,
                PaidDate = updateCommand.PaidDate,
                PaidCreditCard = updateCommand.PaidCreditCard,
                MerchantNumber = updateCommand.MerchantNumber,
                Rrn = updateCommand.Rrn,
                TraceNo = updateCommand.TraceNo,
                RefId = updateCommand.RefId
            };

            await ticketsService.UpdateTicketPaymentAsync(request);

            return Result.Success("پرداخت بلیط با موفقیت ثبت شد");
        }

        catch (Exception ex) when (ex is CustomNotFoundException or AlreadyExistsException)
        {
            return Result.Failure(ex.Message, "درخواست نامعتبر");
        }
    }
}