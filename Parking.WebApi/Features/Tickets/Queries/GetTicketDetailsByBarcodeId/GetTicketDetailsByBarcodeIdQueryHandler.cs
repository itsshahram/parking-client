using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByBarcodeId;

public class GetTicketDetailsByBarcodeIdQueryHandler(
    ITicketsService ticketsService) : IRequestHandler<GetTicketDetailsByBarcodeIdQuery, Result<TicketDetailsResponse>>
{
    public async Task<Result<TicketDetailsResponse>> Handle(GetTicketDetailsByBarcodeIdQuery request, CancellationToken cancellationToken)
    {
        var ticketDetails = await ticketsService.GetTicketDetailsByBarcodeIdAsync(request.BarcodeId);
        
        return ticketDetails is null 
            ? Result<TicketDetailsResponse>.Failure("بلیط با این بارکد وجود ندارد", "بلیط یافت نشد") 
            : Result<TicketDetailsResponse>.Success(ticketDetails, "جزئیات بلیط با موفقیت دریافت شد");
    }
}