using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class CardRepository(ApplicationDbContext context) : GenericRepository<Card>(context), ICardRepository
{
    public async Task<Card?> GetByCardSerialNoAsync(long? cardSerialNo)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.CardSerialNo == cardSerialNo);
    }

    public async Task UpdateCardUsageStatusAsync(long? cardSerialNo, bool isInUse)
    {
        await DbSet
            .Where(c => c.CardSerialNo == cardSerialNo)
            .ExecuteUpdateAsync(update => update.SetProperty(cardUpdate => cardUpdate.IsInUse, isInUse));
    }
}