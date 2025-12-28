using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByBarcodeId;

public class GetTicketDetailsByBarcodeIdQueryHandler(
    ITicketsService ticketsService) : IRequestHandler<GetTicketDetailsByBarcodeIdQuery, Result<TicketDetailsResponse>>
{
    public async Task<Result<TicketDetailsResponse>> Handle(GetTicketDetailsByBarcodeIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var ticketDetails = await ticketsService.GetTicketDetailsByBarcodeIdAsync(request.BarcodeId);

            return Result<TicketDetailsResponse>.Success(ticketDetails, "جزئیات بلیط با موفقیت دریافت شد");
        }
        catch (Exception ex) when(ex is CustomNotFoundException)
        {
            return Result<TicketDetailsResponse>.Failure(ex.Message, "درخواست نامعتبر");
        }
    }
}