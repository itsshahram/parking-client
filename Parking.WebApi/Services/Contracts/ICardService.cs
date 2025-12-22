using Parking.Domain.Entities.Parkings;

namespace Parking.WebApi.Services.Contracts;

public interface ICardService
{
    Task<Card?> GetCardByUidAsync(long cardUid);
}