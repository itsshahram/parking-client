using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tariffs.Queries.GetTariffs;

public class GetTariffsQueryHandler(
    IVehicleSegmentsService vehicleSegmentsService) : IRequestHandler<GetTariffsQuery, Result<List<VehicleSegmentResponse>>>
{
    public async Task<Result<List<VehicleSegmentResponse>>> Handle(GetTariffsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await vehicleSegmentsService.GetAllTariffsAsync();

            return Result<List<VehicleSegmentResponse>>.Success(tariffs, "تعرفه ها با موفقیت دریافت شد");
        }
        catch (Exception ex) when(ex is CustomNotFoundException)
        {
            return Result<List<VehicleSegmentResponse>>.Failure(ex.Message, "درخواست نامعتبر");
        }
    }
}
