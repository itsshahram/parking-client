using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface IParkingVehicleSegmentPriceRepository : IGenericRepository<ParkingVehicleSegmentPrice>
{
    Task<List<ParkingVehicleSegmentPrice>> GetSegmentPricesByParkingSegmentIdAsync(int id);
}