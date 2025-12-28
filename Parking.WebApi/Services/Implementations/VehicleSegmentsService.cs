using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class VehicleSegmentsService(
    IVehicleSegmentRepository vehicleSegmentRepository) : IVehicleSegmentsService
{
    public async Task<List<VehicleSegmentResponse>> GetAllTariffsAsync()
    {
        var tariffs = await vehicleSegmentRepository.GetAllAsync();

        var responses = tariffs
            .Select(vs => new VehicleSegmentResponse
            {
                Id = vs.Id,
                NameFa = vs.NameFa
            })
            .ToList();

        return responses;
    }

    public async Task<VehicleSegmentResponse?> GetTariffByIdAsync(int id)
    {
        var tariff = await vehicleSegmentRepository.GetByIdAsync(id);
        
        var response = tariff is null
            ? throw new CustomNotFoundException("تعرفه با این آی دی وجود ندارد")
            : new VehicleSegmentResponse
            {
                Id = tariff.Id,
                NameFa = tariff.NameFa
            };

        return response;
    }
}