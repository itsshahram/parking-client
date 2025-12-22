using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ITicketExtraImageRepository : IGenericRepository<ParkingTicketExtraImage>
{
    Task AddRangeImagesAsync(List<ParkingTicketExtraImage> ticketExtraImages);
}