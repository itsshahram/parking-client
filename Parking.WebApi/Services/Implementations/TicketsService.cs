using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.General;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Abstractions.UnitOfWork;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Helpers;
using Parking.WebApi.Helpers.PriceCalculation;
using Parking.WebApi.Requests;
using Parking.WebApi.Services.Contracts;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Implementations;

public class TicketsService(
    IParkingVehicleSegmentPriceRepository parkingVehicleSegmentPriceRepository,
    IParkingVehicleSegmentVariablePriceRepository parkingVehicleSegmentVariablePriceRepository,
    IParkingTicketRepository parkingTicketRepository,
    ICardRepository cardRepository,
    IVehicleSegmentRepository vehicleSegmentRepository,
    IParkingLotRepository parkingLotRepository,
    ILicensePlateRepository licensePlateRepository,
    ILicensePlateGroupRepository licensePlateGroupRepository,
    ITicketExtraImageRepository ticketExtraImageRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ITicketsService
{
    public async Task<ParkingTicket?> GetTicketByCardUidAsync(long cardUid)
    {
        return await parkingTicketRepository.GetByCardUidAsync(cardUid);
    }

    public async Task<CreateTicketResponse?> CreateEntryTicketAsync(CreateEntryTicketRequest request)
    {
        var card = await cardRepository.GetByCardSerialNoAsync(request.CardUid);
        if (card is { IsInUse: true })
            return null;
        
        var parkingLot = await parkingLotRepository.GetFirstAsync();
        if (parkingLot == null)
            throw new CustomNotFoundException("پارکینگی یافت نشد");

        var vehicleSegment = await vehicleSegmentRepository.GetByIdAsync(request.VehicleSegmentId);
        if (vehicleSegment == null)
            throw new CustomNotFoundException("تعرفه برای این کارت یافت نشد");
        
        var refinedLicensePlate = ServicesHelpers.RefineLicensePlate(request.EnLicensePlate);
        
        var ticket = new ParkingTicket
        {
            Id = Guid.NewGuid(),
            VehicleManufacturerName = vehicleSegment?.NameFa,
            EnLicensePlate = refinedLicensePlate.EnLicensePlate,
            LicensePlate = refinedLicensePlate.FaLicensePlate,
            DeviceId = string.Empty,
            IP = ServicesHelpers.GetLocalIpAddress(),
            BarcodeId = ServicesHelpers.GenerateRandomBarcodeId(),
            TicketStatus = TicketStatus.Unsynced,
            CardUid = (long?)card!.CardSerialNo,
            ParkingLotId = parkingLot.Id,
            VehicleSegmentId = request.VehicleSegmentId,
            ParkingSpaceID = Guid.Empty,
            ParkingSectionId =  Guid.Empty,
            UserId = currentUserService.UserId,
            IsExited = false,
            IsPaid = false,
            StartTime = DateTime.Now,
            EntranceGate = request.DeviceName
        };

        var licensePlate = await licensePlateRepository.GetByEnLicensePlateAsync(request.EnLicensePlate);
        if (licensePlate != null)
        {
            var licensePlateGroup = await licensePlateGroupRepository.GetActiveByIdAsync(licensePlate.GroupId);
            ticket.LicensePlateGroupId = licensePlateGroup?.Id ?? Guid.Empty;
        }

        if (request.Base64Images == null || request.Base64Images.Count == 0)
        {
            ticket.StartImage = null;
        }
        else
        {
            var images = request.Base64Images;
            ticket.StartImage = images.First();

            if (images.Count > 1)
            {
                var extraImages = images
                    .Skip(1)
                    .Select(base64Image => new ParkingTicketExtraImage
                    {
                        TicketId = ticket.Id,
                        Image = base64Image,
                        CreateDateTime = DateTime.UtcNow,
                        GateName = request.DeviceName,
                        ShowInPage = true
                    })
                    .ToList();

                await ticketExtraImageRepository.AddRangeImagesAsync(extraImages);
            }
        }
        
        await parkingTicketRepository.AddAsync(ticket);
        await cardRepository.UpdateCardUsageStatusAsync(request.CardUid, true);
        
        await unitOfWork.SaveChangesAsync();
        
        return new CreateTicketResponse { TicketId =  ticket.Id, BarcodeId = ticket.BarcodeId.ToString() };
    }
    
    public async Task<TicketDetailsResponse?> GetTicketDetailsByCardUidAsync(long cardUid)
    {
        var ticket = await parkingTicketRepository.GetNotExitedTicketsWithCardUidAsync(cardUid);
        if (ticket is null)
            return null;
        
        var segment = await vehicleSegmentRepository.GetByIdAsync((int)ticket.VehicleSegmentId!);
        var segmentPrices = await parkingVehicleSegmentPriceRepository.GetSegmentPricesByParkingSegmentIdAsync(segment.Id);
        var variableSegmentPrices = await parkingVehicleSegmentVariablePriceRepository.GetVariablePricesByParkingSegmentIdAsync(segment.Id);
        var card = await cardRepository.GetByCardSerialNoAsync(cardUid);
        
        var discount = await licensePlateGroupRepository.GetLicensePlateGroupDiscountWithLicensePlateAsync(ticket.EnLicensePlate);
        
        var varTime = DateTime.Now - ticket.StartTime;
        var description = $"{varTime.Days} روز و {varTime.Hours} ساعت و {varTime.Minutes} دقیقه در {segment.NameFa}";
        
        if (card?.PercentDiscount > 0)
        {
            description += $" | کارت دارای تخفیف {card.PercentDiscount}% است";
            discount = (short)card.PercentDiscount;
        }

        var parkingCostCalculator = new ParkingCostCalculator(
            (int)segment.ParkingEntranceFixedFee, 
            (int)segment.DailyRate,
            segment.FreeEntranceMinutes, 
            segment.ThresholdNumberOfDays, 
            segment.DailyPriceAfterCrossingThreshold, 
            segment.ThresholdHoursPerDay,
            discount, 
            segment.TaxPercentage, 
            segmentPrices,
            variableSegmentPrices);
        
        var calculationResult = parkingCostCalculator.CalculateCost(ticket.StartTime, DateTime.Now);
        
        if (card?.FixDiscount > 0)
        {
            calculationResult.PayableAmount = Math.Max(calculationResult.PayableAmount - card.FixDiscount, 0);
            description += $" | کارت دارای تخفیف {card.FixDiscount} ریال است";
        }

        // update Ticket
        ticket.DurationMinutes = (int)varTime.TotalMinutes;
        ticket.DiscountPercent = (byte)discount;
        ticket.TotalAmount = calculationResult.TotalWithoutDiscount;
        ticket.Description = description;
        ticket.TicketStatus = TicketStatus.Unsynced;
        
        parkingTicketRepository.UpdateTicket(ticket);

        await unitOfWork.SaveChangesAsync();
        
        var images = new List<string>();
        if (!string.IsNullOrEmpty(ticket.StartImage))
            images.Add(ticket.StartImage);
        
        var extraImages = await ticketExtraImageRepository.GetExtraImagesStringAsync(ticket.Id);
        if (extraImages.Count > 0)
            images.AddRange(extraImages);

        return new TicketDetailsResponse
        {
            BarcodeId = ticket.BarcodeId.ToString(),
            EnLicensePlate = ticket.EnLicensePlate,
            FaLicensePlate = ticket.LicensePlate,
            TicketId = ticket.Id.ToString(),
            TotalAmount = calculationResult.PayableAmount,
            Images = images
        };
    }
}                            