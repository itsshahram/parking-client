using MediatR;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.General;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Abstractions.UnitOfWork;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Helpers;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Result<CreateTicketResponse>>
{
    private readonly IParkingTicketRepository _parkingTicketRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IVehicleSegmentRepository _vehicleSegmentRepository;
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly ILicensePlateRepository _licensePlateRepository;
    private readonly ILicensePlateGroupRepository _licensePlateGroupRepository;
    private readonly ITicketExtraImageRepository _ticketExtraImageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketCommandHandler(
        IParkingTicketRepository parkingTicketRepository,
        ICardRepository cardRepository,
        IVehicleSegmentRepository vehicleSegmentRepository,
        IParkingLotRepository parkingLotRepository,
        ILicensePlateRepository licensePlateRepository,
        ILicensePlateGroupRepository licensePlateGroupRepository,
        ITicketExtraImageRepository ticketExtraImageRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _parkingTicketRepository = parkingTicketRepository;
        _cardRepository = cardRepository;
        _vehicleSegmentRepository = vehicleSegmentRepository;
        _parkingLotRepository = parkingLotRepository;
        _licensePlateRepository = licensePlateRepository;
        _licensePlateGroupRepository = licensePlateGroupRepository;
        _ticketExtraImageRepository = ticketExtraImageRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateTicketResponse>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByCardSerialNoAsync(request.CardUid);
        if (card is { IsInUse: true })
            return Result<CreateTicketResponse>.Failure("کارت در حال استفاده است", "صدور بلیط با خطا مواجه شد");

        var parkingLot = await _parkingLotRepository.GetFirstAsync();
        if (parkingLot == null)
            return Result<CreateTicketResponse>.Failure("پارکینگی یافت نشد");

        var vehicleSegment = await _vehicleSegmentRepository.GetByIdAsync(request.VehicleSegmentId);
        if (vehicleSegment == null)
            return Result<CreateTicketResponse>.Failure("تعرفه یافت نشد");

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
            ParkingSectionId = Guid.Empty,
            UserId = _currentUserService.UserId,
            IsExited = false,
            IsPaid = false,
            StartTime = DateTime.Now,
            EntranceGate = request.DeviceName
        };

        var licensePlate = await _licensePlateRepository.GetByEnLicensePlateAsync(request.EnLicensePlate);
        if (licensePlate != null)
        {
            var licensePlateGroup = await _licensePlateGroupRepository.GetActiveByIdAsync(licensePlate.GroupId);
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

                await _ticketExtraImageRepository.AddRangeImagesAsync(extraImages);
            }
        }

        await _parkingTicketRepository.AddAsync(ticket);
        await _cardRepository.UpdateCardUsageStatusAsync(request.CardUid, true);

        await _unitOfWork.SaveChangesAsync();

        var response = new CreateTicketResponse
        {
            TicketId = ticket.Id,
            BarcodeId = ticket.BarcodeId.ToString()
        };

        return Result<CreateTicketResponse>.Success(response, "بلیط پارکینگ با موفقیت ثبت شد");
    }
}
