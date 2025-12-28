using MediatR;
using Parking.WebApi.Application.Common.Models;

namespace Parking.WebApi.Features.Tickets.Commands.UpdateTicketPayment;

public record UpdateTicketPaymentCommand(
    string PaidType,
    List<string>? Base64Images,
    Guid TicketId,
    long Amount,
    string ExitGate,
    string DeviceId,
    DateTime PaidDate,
    string PaidCreditCard,
    string MerchantNumber,
    string Rrn,
    string TraceNo,
    string RefId) : IRequest<Result>;