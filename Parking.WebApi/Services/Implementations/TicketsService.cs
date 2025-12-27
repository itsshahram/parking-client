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
            card = await cardRepository.GetCardByCardSerialNoAsync(request.CardUid!.Value);
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
            DeviceId = request.DeviceId,
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
        await SetTicketImagesAsync(ticket, request.Base64Images, (t, image) => t.StartImage = image, "ورودی");

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

        return await BuildTicketDetailsAsync(ticket, true, cardUid);
    }

    public async Task<TicketDetailsResponse?> GetTicketDetailsByBarcodeIdAsync(long barcodeId)
    {
        var ticket = await parkingTicketRepository.GetNotExitedTicketByBarcodeIdAsync(barcodeId);
        if (ticket == null) 
            return null;

        return await BuildTicketDetailsAsync(ticket, false, barcodeId);
    }

    public async Task UpdateTicketPaymentAsync(PaymentRequest request)
    {
        var ticket = await parkingTicketRepository.GetTicketByIdAsync(request.TicketId);
        
        if (ticket is null)
            throw new CustomNotFoundException("بلیط یافت نشد");
        
        if (ticket.TotalAmountWithDiscount == ticket.PaidAmount && ticket.IsExited)
            throw new AlreadyPaidException("قبلا پرداخت انجام شده است");

        await SetTicketImagesAsync(ticket, request.Base64Images, (t, image) => t.ExitImage = image, "خروجی");
        
        if (ticket.CardUid is not null)
            await cardRepository.UpdateCardUsageStatusAsync(ticket.CardUid, false);
        
        SetTicketPaymentData(ticket, request);

        parkingTicketRepository.UpdateTicket(ticket);
        await unitOfWork.SaveChangesAsync();
    }

    #region Private Helpers

    private void SetTicketPaymentData(ParkingTicket ticket, PaymentRequest request)
    {
        var paidType = Enum.Parse<PaidType>(request.PaidType, true);
        
        ticket.PaidType = paidType.ToString();
        ticket.PaidAmount = request.Amount;
        ticket.PaidDate = request.PaidDate.ToLongDateString();
        ticket.PaidCreditCard = request.PaidCreditCard;
        ticket.MerchantNumber = request.MerchantNumber;
        ticket.RRN = request.Rrn;
        ticket.TraceNo = request.TraceNo;
        ticket.IsPaid = true;
        ticket.ExitRegistrarUserId = currentUserService.UserId;
        ticket.ExitGate = request.ExitGate;
        ticket.EndTime = DateTime.Now;
        ticket.DeviceId = request.DeviceId;
        ticket.IsExited = true;
    }

    private async Task SetLicensePlateGroupAsync(ParkingTicket ticket, string enLicensePlate)
    {
        var licensePlate = await licensePlateRepository.GetByEnLicensePlateAsync(enLicensePlate);
        if (licensePlate != null)
        {
            var licensePlateGroup = await licensePlateGroupRepository.GetActiveByIdAsync(licensePlate.GroupId);
            ticket.LicensePlateGroupId = licensePlateGroup?.Id ?? Guid.Empty;
        }
    }
    
    private async Task SetTicketImagesAsync(
        ParkingTicket ticket,
        List<string>? base64Images,
        Action<ParkingTicket, string?> setMainImage,
        string imageTypePrefix)
    {
        if (base64Images == null || base64Images.Count == 0)
        {
            setMainImage(ticket, null);
            return;
        }

        setMainImage(ticket, base64Images.First());

        if (base64Images.Count > 1)
        {
            var now = DateTime.UtcNow;

            var extraImages = base64Images
                .Skip(1)
                .Select((img, index) => new ParkingTicketExtraImage
                {
                    TicketId = ticket.Id,
                    Image = img,
                    FaName = $"عکس_{imageTypePrefix}_پوز_{ticket.LicensePlate}_{index + 1:D2}",
                    CreateDateTime = now,
                    GateName = ticket.DeviceId,
                    ShowInPage = true
                })
                .ToList();

            await ticketExtraImageRepository.AddRangeImagesAsync(extraImages);
        }
    }

    private async Task<TicketDetailsResponse> BuildTicketDetailsAsync(ParkingTicket ticket,bool isCardUid, long cardUidOrBarcodeId)
    {
        var segment = await vehicleSegmentRepository.GetByIdAsync((int)ticket.VehicleSegmentId!);
        var segmentPrices = await parkingVehicleSegmentPriceRepository.GetSegmentPricesByParkingSegmentIdAsync(segment.Id);
        var variableSegmentPrices = await parkingVehicleSegmentVariablePriceRepository.GetVariablePricesByParkingSegmentIdAsync(segment.Id);

        var varTime = DateTime.Now - ticket.StartTime;
        var discount = await licensePlateGroupRepository.GetLicensePlateGroupDiscountWithLicensePlateAsync(ticket.EnLicensePlate);
        var description = $"{varTime.Days} روز و {varTime.Hours} ساعت و {varTime.Minutes} دقیقه در {segment.NameFa}";

        Card? card = null;
        
        if (isCardUid)
        {
            card = await cardRepository.GetCardByCardSerialNoAsync(cardUidOrBarcodeId);
            
            if (card is null)
                throw new CustomNotFoundException("کارت با این شناسه وجود ندارد");
            
            if (card.PercentDiscount > 0)
            {
                description += $" | کارت دارای تخفیف {card.PercentDiscount}% است";
                discount = (short)card.PercentDiscount;
            }
        }
        else
        {
            discount = ticket.DiscountPercent;
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
        
        if (card is not null && card.FixDiscount > 0)
        {
            calculationResult.PayableAmount = Math.Max(calculationResult.PayableAmount - card.FixDiscount, 0);
            description += $" | کارت دارای تخفیف {card.FixDiscount} ریال است";
        }
        
        ticket.TotalAmount = calculationResult.TotalWithoutDiscount;
        ticket.TotalAmountWithDiscount = calculationResult.PayableAmount;
        ticket.DurationMinutes = (int)varTime.TotalMinutes;
        ticket.DiscountPercent = (byte)discount;
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
            TotalAmount = calculationResult.TotalWithoutDiscount,
            PayableAmount = calculationResult.PayableAmount,
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