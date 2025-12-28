using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class ParkingVehicleSegmentPriceRepository(
    ApplicationDbContext context) : GenericRepository<ParkingVehicleSegmentPrice>(context), IParkingVehicleSegmentPriceRepository
{
    public async Task<List<ParkingVehicleSegmentPrice>> GetSegmentPricesByParkingSegmentIdAsync(int vehicleSegmentId)
    {
        return await DbSet
            .Where(p => p.VehicleSegmentId == vehicleSegmentId)
            .ToListAsync();
    }
}