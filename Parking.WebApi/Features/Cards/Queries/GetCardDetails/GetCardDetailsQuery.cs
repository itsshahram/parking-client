using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public record GetCardDetailsQuery(long CardUid) : IRequest<Result<PlateAndTariffResponse>>;
