using Parking.Core.Engine;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.Vehicles;

namespace Parking.Core.Service;

public interface IParkingPriceService
{
    Task<PricingEngine> CreateEngine(
        VehicleSegment segment,
        List<SpecialRule> specialRules,
        List<ParkingVehicleSegmentPrice> segmentPrices,
        List<ParkingVehicleSegmentVariablePrice> variablePrices,
        decimal discountPercentage);
}