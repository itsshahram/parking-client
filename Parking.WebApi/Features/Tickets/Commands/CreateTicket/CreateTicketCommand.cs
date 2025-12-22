using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Tickets.Commands.CreateTicket;

public record CreateTicketCommand(
    string EnLicensePlate,
    PlateType PlateType,
    int VehicleSegmentId,
    string DeviceName,
    List<string>? Base64Images,
    long? CardUid) : IRequest<Result<CreateTicketResponse>>;
