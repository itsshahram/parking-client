using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;
public interface IParkingTicketRepository : IGenericRepository<ParkingTicket>
{
    Task<ParkingTicket?> GetByCardUidAsync(decimal? cardUid);
    Task<ParkingTicket?> GetTicketByIdAsync(Guid ticketId);
    Task<ParkingTicket?> GetNotExitedTicketWithCardUidAsync(decimal? cardUid);
    Task<ParkingTicket?> GetNotExitedTicketByBarcodeIdAsync(long barcodeId);
    Task<bool> IsExistedLicensePlate(string licensePlate);
    void UpdateTicket(ParkingTicket ticket);
}