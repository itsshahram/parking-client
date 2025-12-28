using Parking.Domain.Entities.Parkings;

namespace Parking.WebApi.Services.Contracts;

public interface ICardService
{
    Task<Card> GetCardByCardUidAsync(long cardSerialNo);
    Task UseCardAsync(long cardUid);
    Task ReleaseCardAsync(long cardUid);
}