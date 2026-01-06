using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class SpecialRulesRepository(ApplicationDbContext context) : GenericRepository<SpecialRule>(context), ISpecialRulesRepository
{
    public Task<List<SpecialRule>> GetActiveSpecialRulesAsync()
    {
        return DbSet.Where(sp => sp.IsActive).ToListAsync();
    }
}