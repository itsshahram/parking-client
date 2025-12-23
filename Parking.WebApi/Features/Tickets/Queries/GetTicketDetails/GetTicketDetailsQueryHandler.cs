using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetails;

public class GetTicketDetailsQueryHandler(
    ITicketsService ticketsService) : IRequestHandler<GetTicketDetailsQuery, Result<TicketDetailsResponse>>
{
    public async Task<Result<TicketDetailsResponse>> Handle(GetTicketDetailsQuery request, CancellationToken cancellationToken)
    {
        var ticketDetails = await ticketsService.GetTicketDetailsByCardUidAsync(request.CardUid);
        
        return ticketDetails is null 
            ? Result<TicketDetailsResponse>.Failure("کارت با این شناسه وجود ندارد", "کارت یافت نشد") 
            : Result<TicketDetailsResponse>.Success(ticketDetails, "جزئیات بلیط با موفقیت دریافت شد");
    }
}