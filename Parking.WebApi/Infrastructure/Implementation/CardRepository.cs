using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class CardRepository(ApplicationDbContext context) : GenericRepository<Card>(context), ICardRepository
{
    public async Task<Card?> GetCardByCardSerialNoOrBarcodeIdAsync(long? cardSerialNo)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.CardSerialNo == cardSerialNo);
    }

    public async Task UpdateCardUsageStatusAsync(decimal? cardSerialNo, bool isInUse)
    {
        if (!cardSerialNo.HasValue)
            return;

        await DbSet
            .Where(c => c.CardSerialNo == cardSerialNo.Value)
            .ExecuteUpdateAsync(update =>
                update.SetProperty(c => c.IsInUse, isInUse));
    }
}