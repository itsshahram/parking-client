using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class SeizedLicensePlateRepository(ApplicationDbContext context) : GenericRepository<SeizedLicensePlate>(context), ISeizedLicensePlateRepository
{
    public Task<bool> IsLicensePlateSeizedAsync(string licensePlate)
    {
        return DbSet.AnyAsync(l => l.EnLicensePlate == licensePlate);
    }
}