using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tariffs.Queries.GetTariffs;

public class GetTariffsQueryHandler : IRequestHandler<GetTariffsQuery, Result<List<VehicleSegmentResponse>>>
{
    private readonly IVehicleSegmentsService _vehicleSegmentsService;

    public GetTariffsQueryHandler(IVehicleSegmentsService vehicleSegmentsService)
    {
        _vehicleSegmentsService = vehicleSegmentsService;
    }

    public async Task<Result<List<VehicleSegmentResponse>>> Handle(GetTariffsQuery request, CancellationToken cancellationToken)
    {
        var tariffs = await _vehicleSegmentsService.GetAllTariffsAsync();
        return Result<List<VehicleSegmentResponse>>.Success(tariffs, "تعرفه ها با موفقیت دریافت شد");
    }
}
