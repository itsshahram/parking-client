using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class TicketExtraImageRepository(
    ApplicationDbContext context) : GenericRepository<ParkingTicketExtraImage>(context), ITicketExtraImageRepository
{
    public async Task AddRangeImagesAsync(List<ParkingTicketExtraImage> ticketExtraImages)
    {
        await DbSet.AddRangeAsync(ticketExtraImages);
    }
}