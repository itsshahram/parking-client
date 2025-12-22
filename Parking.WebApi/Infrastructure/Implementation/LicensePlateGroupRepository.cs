using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class LicensePlateGroupRepository(
    ApplicationDbContext context) : GenericRepository<LicensePlateGroup>(context), ILicensePlateGroupRepository
{
    public async Task<LicensePlateGroup?> GetActiveByIdAsync(Guid? groupId)
    {
        return await DbSet.FirstOrDefaultAsync(g => 
            g.Id == groupId &&
            g.StartDate < DateTime.Now);
    }
}