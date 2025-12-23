using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public class GetCardDetailsQueryHandler(
    ICardService cardService) : IRequestHandler<GetCardDetailsQuery, Result<PlateAndTariffResponse>>
{
    public async Task<Result<PlateAndTariffResponse>> Handle(GetCardDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await cardService.GetCardByUidAsync(request.CardUid);
            
            return Result<PlateAndTariffResponse>.Success(response, "جزییات کارت با موفقیت دریافت شد");
        }
        
        catch (CustomNotFoundException ex)
        {
            return Result<PlateAndTariffResponse>.Failure(ex.Message, "یافت نشد");
        }
        catch (InActiveCardException ex)
        {
            return Result<PlateAndTariffResponse>.Failure(ex.Message, "کارت غیرفعال");
        }
    }
}
