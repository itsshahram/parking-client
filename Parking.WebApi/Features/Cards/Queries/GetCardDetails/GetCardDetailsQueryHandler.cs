using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public class GetCardDetailsQueryHandler(
    ITicketsService ticketsService) : IRequestHandler<GetCardDetailsQuery, Result<PlateAndTariffResponse>>
{
    public async Task<Result<PlateAndTariffResponse>> Handle(GetCardDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await ticketsService.GetPlateAndTariffAsync(request.CardUid);

            return Result<PlateAndTariffResponse>.Success(response, "جزییات کارت با موفقیت دریافت شد");
        }

        catch (Exception ex) when (ex is CustomNotFoundException or InActiveCardException or CardIsInUseException)
        {
            return Result<PlateAndTariffResponse>.Failure(ex.Message, "درخواست نامعتبر");
        }
    }
}
