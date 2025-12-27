using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByCardUid;

public class GetTicketDetailsByCardUidQueryHandler(
    ITicketsService ticketsService) : IRequestHandler<GetTicketDetailsByCardUidQuery, Result<TicketDetailsResponse>>
{
    public async Task<Result<TicketDetailsResponse>> Handle(GetTicketDetailsByCardUidQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var ticketDetails = await ticketsService.GetTicketDetailsByCardUidAsync(request.CardUid);

            return ticketDetails is null
                ? Result<TicketDetailsResponse>.Failure("بلیط با این شناسه وجود ندارد", "بلیط یافت نشد")
                : Result<TicketDetailsResponse>.Success(ticketDetails, "جزئیات بلیط با موفقیت دریافت شد");
        }
        catch (CustomNotFoundException ex)
        {
            return Result<TicketDetailsResponse>.Failure(ex.Message, "یافت نشد");
        }
    }
}