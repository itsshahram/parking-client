using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Contracts;

public interface ICardService
{
    Task<PlateAndTariffResponse> GetCardByUidAsync(long cardUid);
}