using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ICardRepository : IGenericRepository<Card>
{
    Task<Card?> GetByCardSerialNoAsync(long? cardSerialNo);
    Task UpdateCardUsageStatusAsync(long? cardSerialNo, bool isInUse);
}