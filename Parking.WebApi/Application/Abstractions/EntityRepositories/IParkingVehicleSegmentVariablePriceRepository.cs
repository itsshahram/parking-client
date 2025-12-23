using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface IParkingVehicleSegmentVariablePriceRepository : IGenericRepository<ParkingVehicleSegmentVariablePrice>
{
    Task<List<ParkingVehicleSegmentVariablePrice>> GetVariablePricesByParkingSegmentIdAsync(int id);
}