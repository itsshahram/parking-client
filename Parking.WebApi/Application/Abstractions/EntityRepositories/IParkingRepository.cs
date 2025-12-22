using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;
public interface IParkingTicketRepository : IGenericRepository<ParkingTicket>
{
    Task<ParkingTicket?> GetByCardUidAsync(decimal? cardUid);
}