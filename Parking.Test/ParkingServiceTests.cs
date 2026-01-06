using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Parking.Core.Service;
using Parking.Domain.Entities.Vehicles;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Abstractions.UnitOfWork;
using Parking.WebApi.Services.Contracts;
using Parking.WebApi.Services.Implementations;
using Xunit.Abstractions;

namespace Parking.Test;

public class TicketsServiceTest(
    TestFixture fixture,
    ITestOutputHelper output) : IClassFixture<TestFixture>
{
    // [Fact]
    // public async Task GetAllAsync()
    // {
    //     var vehicleSegmentRepository = fixture.ServiceProvider.GetRequiredService<IVehicleSegmentRepository>();
    //     var result = await vehicleSegmentRepository.GetAllAsync();
    //     output.WriteLine("Result of get all is {0}",JsonConvert.SerializeObject(result));
    //     
    //     Assert.True(result.Any());
    // }
    
    [Fact]
    public async Task CalculateParkingPrice_Returns_Result()
    {
        // Arrange
        var mockParkingVehicleSegmentPriceRepository = new Mock<IParkingVehicleSegmentPriceRepository>();
        var mockParkingVehicleSegmentVariablePriceRepository = new Mock<IParkingVehicleSegmentVariablePriceRepository>();
        var mockParkingPriceService = new Mock<IParkingPriceService>();
        var mockParkingTicketRepository = new Mock<IParkingTicketRepository>();
        var mockVehicleSegmentRepository = new Mock<IVehicleSegmentRepository>();
        var mockParkingLotRepository = new Mock<IParkingLotRepository>();
        var mockLicensePlateRepository = new Mock<ILicensePlateRepository>();
        var mockLicensePlateGroupRepository = new Mock<ILicensePlateGroupRepository>();
        var mockTicketExtraImageRepository = new Mock<ITicketExtraImageRepository>();
        var mockSeizedLicensePlateRepository = new Mock<ISeizedLicensePlateRepository>();
        var mockCardService = new Mock<ICardService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var ticketsService = new TicketsService(
            mockParkingVehicleSegmentPriceRepository.Object,
            mockParkingVehicleSegmentVariablePriceRepository.Object,
            mockParkingTicketRepository.Object,
            mockVehicleSegmentRepository.Object,
            mockParkingLotRepository.Object,
            mockLicensePlateRepository.Object,
            mockLicensePlateGroupRepository.Object,
            mockTicketExtraImageRepository.Object,
            mockSeizedLicensePlateRepository.Object,
            mockCardService.Object,
            mockCurrentUserService.Object,
            mockParkingPriceService.Object,
            mockUnitOfWork.Object
        );

        var vehicleSegmentRepository = fixture.ServiceProvider.GetRequiredService<IVehicleSegmentRepository>();
        var segmentPriceRepository = fixture.ServiceProvider.GetRequiredService<IParkingVehicleSegmentPriceRepository>();
        var segmentVariablePriceRepository = fixture.ServiceProvider.GetRequiredService<IParkingVehicleSegmentVariablePriceRepository>();
        var actualParkingPriceService = fixture.ServiceProvider.GetRequiredService<IParkingPriceService>();

        const int testId = 40;
        
        var segment = await vehicleSegmentRepository.GetByIdAsync(testId);
        var segmentPrices = await segmentPriceRepository.GetSegmentPricesByParkingSegmentIdAsync(segment.Id);
        var variableSegmentPrices = await segmentVariablePriceRepository.GetVariablePricesByParkingSegmentIdAsync(segment.Id);

        mockVehicleSegmentRepository
            .Setup(x => x.GetByIdAsync(testId))
            .ReturnsAsync(segment);

        mockParkingVehicleSegmentPriceRepository
            .Setup(x => x.GetSegmentPricesByParkingSegmentIdAsync(segment.Id))
            .ReturnsAsync(segmentPrices);

        mockParkingVehicleSegmentVariablePriceRepository
            .Setup(x => x.GetVariablePricesByParkingSegmentIdAsync(segment.Id))
            .ReturnsAsync(variableSegmentPrices);

        const int discount = 0;

        var realPricingEngine = await actualParkingPriceService.CreateEngine(
            segment, 
            null, 
            segmentPrices, 
            variableSegmentPrices, 
            discount);

        mockParkingPriceService
            .Setup(x => x.CreateEngine(segment, null, segmentPrices, variableSegmentPrices, discount))
            .ReturnsAsync(realPricingEngine);

        // Act
        var entryDate = new DateTime(2025, 10, 01, 10, 0, 0);
        var exitDate = new DateTime(2025, 10, 02, 10, 0, 0);

        var result = await InvokeCalculateParkingPrice(ticketsService, entryDate, exitDate, segment, discount);
        
        output.WriteLine("Result: {0}", JsonConvert.SerializeObject(result));

        // Assert
        Assert.InRange(result, 0, 60000000m);
        
        mockParkingPriceService
            .Verify(x => x.CreateEngine(segment, null, segmentPrices, variableSegmentPrices, discount), Times.Once);
    }
    
    
    private static async Task<decimal> InvokeCalculateParkingPrice(
        TicketsService ticketsService,
        DateTime entryDate,
        DateTime? exitDate,
        VehicleSegment segment,
        decimal discount)
    {
        var methodInfo = typeof(TicketsService).GetMethod("CalculateParkingPrice", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (methodInfo == null)
            throw new InvalidOperationException("CalculateParkingPrice method not found");

        var task = (Task<decimal>)methodInfo.Invoke(ticketsService, new object[] { entryDate, exitDate, segment, discount })!;
        return await task;
    }
}