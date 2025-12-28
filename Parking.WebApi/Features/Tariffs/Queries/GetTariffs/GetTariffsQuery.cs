using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Tariffs.Queries.GetTariffs;

public record GetTariffsQuery : IRequest<Result<List<VehicleSegmentResponse>>>;
