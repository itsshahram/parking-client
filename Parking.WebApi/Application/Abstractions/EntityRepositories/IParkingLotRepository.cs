using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface IParkingLotRepository : IGenericRepository<ParkingLot>
{
    Task<ParkingLot?> GetFirstAsync();
}