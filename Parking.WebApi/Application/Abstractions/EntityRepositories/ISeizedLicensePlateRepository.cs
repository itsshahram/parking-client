using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ISeizedLicensePlateRepository : IGenericRepository<SeizedLicensePlate>
{
    Task<bool> IsLicensePlateSeizedAsync(string licensePlate);
}