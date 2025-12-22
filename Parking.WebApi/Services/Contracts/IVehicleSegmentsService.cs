using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Contracts;

public interface IVehicleSegmentsService
{
    Task<List<VehicleSegmentResponse>> GetAllTariffsAsync();
    Task<VehicleSegmentResponse?> GetTariffByIdAsync(int id);
}
