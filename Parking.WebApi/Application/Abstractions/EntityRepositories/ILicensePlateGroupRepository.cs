using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Infrastructure.Repository;

namespace Parking.WebApi.Application.Abstractions.EntityRepositories;

public interface ILicensePlateGroupRepository : IGenericRepository<LicensePlateGroup>
{
    Task<LicensePlateGroup?> GetActiveByIdAsync(Guid? groupId);
}