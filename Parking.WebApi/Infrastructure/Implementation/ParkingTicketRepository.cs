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
        return await DbSet.FirstOrDefaultAsync(t => t.CardUid == cardUid);
    }

    public async Task<ParkingTicket?> GetNotExitedTicketsWithCardUidAsync(decimal? cardUid)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.CardUid == cardUid && !t.IsExited);
    }

    public void UpdateTicket(ParkingTicket ticket)
    {
        DbSet.Update(ticket);
    }
}