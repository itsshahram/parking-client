using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByBarcodeId;

public record GetTicketDetailsByBarcodeIdQuery(long BarcodeId) : IRequest<Result<TicketDetailsResponse>>;