using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class CardService(
    ICardRepository cardRepository) : ICardService
{
    public async Task<Card> GetCardByCardUidAsync(long cardSerialNo)
    {
        var card = await cardRepository.GetCardByCardSerialNoAsync(cardSerialNo);
        
        if (card is null)
            throw new CustomNotFoundException("کارت با این شناسه وجود ندارد");
        
        return !card.IsActive 
            ? throw new InActiveCardException("کارت غیرفعال است و نمی توان از آن استفاده کرد") 
            : card;
    }
    
    public async Task UseCardAsync(long cardUid)
    {
        var card = await GetCardByCardUidAsync(cardUid);

        card.IsInUse = true;
        
        await cardRepository.UpdateCardUsageStatusAsync(cardUid, true);
    }
    
    public async Task ReleaseCardAsync(long cardUid)
    {
        var card = await GetCardByCardUidAsync(cardUid);

        card.IsInUse = false;
        
        await cardRepository.UpdateCardUsageStatusAsync(cardUid, false);
    }
}