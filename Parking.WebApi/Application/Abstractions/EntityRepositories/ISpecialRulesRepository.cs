using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ISpecialRulesRepository : IGenericRepository<SpecialRule>
{
    Task<List<SpecialRule>> GetActiveSpecialRulesAsync();
}