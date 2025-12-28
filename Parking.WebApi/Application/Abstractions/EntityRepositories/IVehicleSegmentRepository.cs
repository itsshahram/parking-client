using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;
public interface IVehicleSegmentRepository : IGenericRepository<VehicleSegment>
{
    Task<VehicleSegment?> GetByIdAsync(int id);
    new Task<List<VehicleSegment>> GetAllAsync();
}