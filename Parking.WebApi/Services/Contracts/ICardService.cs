using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Contracts;

public interface ICardService
{
    Task<Card> GetCardByCardUidAsync(long cardSerialNo);
    Task UseCardAsync(long cardUid);
    Task ReleaseCardAsync(long cardUid);
}