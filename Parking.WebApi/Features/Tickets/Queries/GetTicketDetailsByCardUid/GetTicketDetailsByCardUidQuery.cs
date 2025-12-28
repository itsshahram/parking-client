using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByCardUid;

public record GetTicketDetailsByCardUidQuery(long CardUid) : IRequest<Result<TicketDetailsResponse>>;