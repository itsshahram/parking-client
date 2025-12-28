using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class VehicleSegmentRepository(
    ApplicationDbContext context) : GenericRepository<VehicleSegment>(context), IVehicleSegmentRepository
{
    public async Task<VehicleSegment?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public override async Task<List<VehicleSegment>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }
}