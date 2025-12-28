using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class ParkingVehicleSegmentVariablePriceRepository(
    ApplicationDbContext context) : GenericRepository<ParkingVehicleSegmentVariablePrice>(context), IParkingVehicleSegmentVariablePriceRepository
{
    public async Task<List<ParkingVehicleSegmentVariablePrice>> GetVariablePricesByParkingSegmentIdAsync(int vehicleSegmentId)
    {
        return await DbSet
            .Where(p => p.VehicleSegmentId == vehicleSegmentId)
            .ToListAsync();
    }
}