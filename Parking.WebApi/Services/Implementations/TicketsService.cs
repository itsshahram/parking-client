using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.General;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Abstractions.UnitOfWork;
using Parking.WebApi.Helpers;
using Parking.WebApi.Requests;
using Parking.WebApi.Services.Contracts;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Implementations;

public class TicketsService(
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
    public async Task<CreateTicketResponse> CreateEntryTicketAsync(CreateEntryTicketRequest request)
    {
        var card = await cardRepository.GetByCardSerialNoAsync(request.CardUid);
        if (card is { IsInUse: true })
            return new CreateTicketResponse { TicketId = Guid.Empty, BarcodeId = string.Empty };
        
        var parkingLot = await parkingLotRepository.GetFirstAsync();
        if (parkingLot == null)
            throw new InvalidOperationException("No parking lot found");

        var vehicleSegment = await vehicleSegmentRepository.GetByIdAsync(request.VehicleSegmentId);
        if (vehicleSegment == null)
            throw new InvalidOperationException("Vehicle segment not found");
        
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
}                            