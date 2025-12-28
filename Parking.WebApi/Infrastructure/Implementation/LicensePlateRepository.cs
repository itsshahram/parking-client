using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class LicensePlateRepository(
    ApplicationDbContext context) : GenericRepository<LicensePlate>(context), ILicensePlateRepository
{
    public async Task<LicensePlate?> GetByEnLicensePlateAsync(string enLicensePlate)
    {
        return await DbSet.FirstOrDefaultAsync(lp => lp.EnLicensePlate == enLicensePlate);
    }
}