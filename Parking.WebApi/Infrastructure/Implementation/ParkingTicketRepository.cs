using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class ParkingTicketRepository(
    ApplicationDbContext context) : GenericRepository<ParkingTicket>(context), IParkingTicketRepository
{
    public async Task<ParkingTicket?> GetByCardUidAsync(decimal? cardUid)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.CardUid == cardUid && !t.IsExited);
    }

    public async Task<ParkingTicket?> GetTicketByIdAsync(Guid ticketId)
    {
        return await DbSet.FindAsync(ticketId);
    }

    public async Task<ParkingTicket?> GetNotExitedTicketWithCardUidAsync(decimal? cardUid)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.CardUid == cardUid && !t.IsExited);
    }

    public async Task<ParkingTicket?> GetNotExitedTicketByBarcodeIdAsync(long barcodeId)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.BarcodeId == barcodeId && !t.IsExited);
    }

    public async Task<bool> IsExistedLicensePlate(string licensePlate)
    {
        return await DbSet.AnyAsync(t => t.EnLicensePlate == licensePlate && !t.IsExited);
    }

    public void UpdateTicket(ParkingTicket ticket)
    {
        DbSet.Update(ticket);
    }
}