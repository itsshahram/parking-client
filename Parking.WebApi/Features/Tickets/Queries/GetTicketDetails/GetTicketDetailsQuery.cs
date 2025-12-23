using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetails;

public record GetTicketDetailsQuery(long CardUid) : IRequest<Result<TicketDetailsResponse>>;