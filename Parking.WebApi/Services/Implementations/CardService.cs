using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class CardService(
    ApplicationDbContext context) : ICardService
{
    public Task<Card?> GetCardByUidAsync(long cardUid)
    {
        var card = context
            .Cards
            .FirstOrDefaultAsync(c => c.CardSerialNo == cardUid);

        return card;
    }
}