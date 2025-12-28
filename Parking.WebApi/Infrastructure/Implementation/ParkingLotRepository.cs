using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.Parkings;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Infrastructure.Implementation;

public class ParkingLotRepository(
    ApplicationDbContext context) : GenericRepository<ParkingLot>(context), IParkingLotRepository
{
    public async Task<ParkingLot?> GetFirstAsync()
    {
        return await DbSet.FirstOrDefaultAsync();
    }
}