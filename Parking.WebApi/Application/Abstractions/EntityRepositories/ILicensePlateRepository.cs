using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ILicensePlateRepository : IGenericRepository<LicensePlate>
{
    Task<LicensePlate?> GetByEnLicensePlateAsync(string enLicensePlate);
}