using Parking.Domain.Entities.Parkings;
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
        => await parkingTicketRepository.GetByCardUidAsync(cardUid);

    public async Task<CreateTicketResponse?> CreateEntryTicketAsync(CreateEntryTicketRequest request)
    {
        decimal? cardSerialNo = request.CardUid.HasValue ? request.CardUid.Value : null;
        Card? card = null;

        if (cardSerialNo.HasValue)
        {
            card = await cardRepository.GetCardByCardSerialNoOrBarcodeIdAsync(request.CardUid!.Value);
            if (card?.IsInUse == true) return null;
        }
        
        var isExistedLicensePlate = await parkingTicketRepository.IsExistedLicensePlate(request.EnLicensePlate);
        if (isExistedLicensePlate)
            throw new AlreadyExistsException("این پلاک قبلا ثبت شده است");

        var parkingLot = await parkingLotRepository.GetFirstAsync()
            ?? throw new CustomNotFoundException("پارکینگی یافت نشد");

        var vehicleSegment = await vehicleSegmentRepository.GetByIdAsync(request.VehicleSegmentId)
            ?? throw new CustomNotFoundException("تعرفه برای این کارت یافت نشد");

        var refinedLicensePlate = ServicesHelpers.RefineLicensePlate(request.EnLicensePlate);

        var ticket = new ParkingTicket
        {
            Id = Guid.NewGuid(),
            VehicleManufacturerName = vehicleSegment.NameFa,
            EnLicensePlate = refinedLicensePlate.EnLicensePlate,
            LicensePlate = refinedLicensePlate.FaLicensePlate,
            DeviceId = string.Empty,
            IP = ServicesHelpers.GetLocalIpAddress(),
            BarcodeId = ServicesHelpers.GenerateRandomBarcodeId(),
            TicketStatus = TicketStatus.Unsynced,
            CardUid = request.CardUid,
            ParkingLotId = parkingLot.Id,
            VehicleSegmentId = request.VehicleSegmentId,
            ParkingSpaceID = Guid.Empty,
            ParkingSectionId = Guid.Empty,
            UserId = currentUserService.UserId,
            IsExited = false,
            IsPaid = false,
            StartTime = DateTime.Now,
            EntranceGate = request.DeviceName
        };

        await SetLicensePlateGroupAsync(ticket, request.EnLicensePlate);
        await SetTicketImagesAsync(ticket, request.Base64Images);

        await parkingTicketRepository.AddAsync(ticket);

        if (card != null)
            await cardRepository.UpdateCardUsageStatusAsync(cardSerialNo, true);

        await unitOfWork.SaveChangesAsync();

        return new CreateTicketResponse { TicketId = ticket.Id, BarcodeId = ticket.BarcodeId.ToString() };
    }

    public async Task<TicketDetailsResponse?> GetTicketDetailsByCardUidAsync(long cardUid)
    {
        var ticket = await parkingTicketRepository.GetNotExitedTicketWithCardUidAsync(cardUid);
        if (ticket == null) 
            return null;

        return await BuildTicketDetailsAsync(ticket, cardUid);
    }

    public async Task<TicketDetailsResponse?> GetTicketDetailsByBarcodeIdAsync(long barcodeId)
    {
        var ticket = await parkingTicketRepository.GetNotExitedTicketByBarcodeIdAsync(barcodeId);
        if (ticket == null) 
            return null;

        return await BuildTicketDetailsAsync(ticket, barcodeId);
    }
    
    #region Private Helpers

    private async Task SetLicensePlateGroupAsync(ParkingTicket ticket, string enLicensePlate)
    {
        var licensePlate = await licensePlateRepository.GetByEnLicensePlateAsync(enLicensePlate);
        if (licensePlate != null)
        {
            var licensePlateGroup = await licensePlateGroupRepository.GetActiveByIdAsync(licensePlate.GroupId);
            ticket.LicensePlateGroupId = licensePlateGroup?.Id ?? Guid.Empty;
        }
    }

    private async Task SetTicketImagesAsync(ParkingTicket ticket, List<string>? base64Images)
    {
        if (base64Images == null || base64Images.Count == 0)
        {
            ticket.StartImage = null;
            return;
        }

        ticket.StartImage = base64Images.First();

        if (base64Images.Count > 1)
        {
            var extraImages = base64Images
                .Skip(1)
                .Select(img => new ParkingTicketExtraImage
                {
                    TicketId = ticket.Id,
                    Image = img,
                    CreateDateTime = DateTime.UtcNow,
                    GateName = ticket.DeviceId,
                    ShowInPage = true
                })
                .ToList();

            await ticketExtraImageRepository.AddRangeImagesAsync(extraImages);
        }
    }

    private async Task<TicketDetailsResponse> BuildTicketDetailsAsync(ParkingTicket ticket, long cardOrBarcodeId)
    {
        var segment = await vehicleSegmentRepository.GetByIdAsync((int)ticket.VehicleSegmentId!);
        var segmentPrices = await parkingVehicleSegmentPriceRepository.GetSegmentPricesByParkingSegmentIdAsync(segment.Id);
        var variableSegmentPrices = await parkingVehicleSegmentVariablePriceRepository.GetVariablePricesByParkingSegmentIdAsync(segment.Id);
        var card = await cardRepository.GetCardByCardSerialNoOrBarcodeIdAsync(cardOrBarcodeId);

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

        ticket.DurationMinutes = (int)varTime.TotalMinutes;
        ticket.DiscountPercent = (byte)discount;
        ticket.TotalAmount = calculationResult.TotalWithoutDiscount;
        ticket.Description = description;
        ticket.TicketStatus = TicketStatus.Unsynced;

        parkingTicketRepository.UpdateTicket(ticket);
        await unitOfWork.SaveChangesAsync();

        var images = await GetTicketImagesAsync(ticket);

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

    private async Task<List<string>> GetTicketImagesAsync(ParkingTicket ticket)
    {
        var images = new List<string>();
        if (!string.IsNullOrEmpty(ticket.StartImage))
            images.Add(ticket.StartImage);

        var extraImages = await ticketExtraImageRepository.GetExtraImagesStringAsync(ticket.Id);
        if (extraImages.Count > 0)
            images.AddRange(extraImages);

        return images;
    }

    #endregion
}                            