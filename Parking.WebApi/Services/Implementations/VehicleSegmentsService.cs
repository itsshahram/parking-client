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
}