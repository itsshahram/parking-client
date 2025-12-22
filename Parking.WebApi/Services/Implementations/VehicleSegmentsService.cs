using Microsoft.EntityFrameworkCore;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class VehicleSegmentsService(
    ApplicationDbContext context) : IVehicleSegmentsService
{
    public Task<List<VehicleSegmentResponse>> GetAllTariffsAsync()
    {
        var vehicleSegments = context
            .VehicleSegments
            .Select(vs => new VehicleSegmentResponse
            {
                Id = vs.Id,
                NameFa = vs.NameFa
            })
            .ToListAsync();

        return vehicleSegments;
    }

    public async Task<VehicleSegmentResponse?> GetTariffByIdAsync(int id)
    {
        var tariff = await context.VehicleSegments
            .Where(v => v.Id == id)
            .Select(v => new VehicleSegmentResponse
            {
                Id = v.Id,
                NameFa = v.NameFa
            })
            .FirstOrDefaultAsync();

        return tariff;
    }
}