using Parking.App.Models.Dto.Card;
using Parking.App.Models.Dto.Parking.ParkingLot;
using Parking.App.Models.Dto.Parking.ParkingSection;
using Parking.App.Models.Dto.Parking.ParkingSpace;
using Parking.App.Models.Dto.Parking.ParkingTicket;
using Parking.App.Models.Dto.Vehicle.LicensePlate;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.GeneralServiceResponse;
using Parking.App.Models.Tickets;
using Parking.App.Models;
using Parking.Domain.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Parking.App.Helpers;

namespace Parking.App.Services.Interfaces;

public interface IParkingService
{
    TServiceResponse<ParkingLotModel> GetParkingLotDetails();
    Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceList();
    Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceListAsync();
    List<VehicleSegmentModel> GetVehicleSegments();
    List<ParkingSectionModel> GetSections();
    List<ParkingSpaceModel> GetSpaces();
    int GetFreeSpacesCount();
    int GetSpacesCount();
    (Guid? SpaceId, Guid? SectionId) GetOneFreeSpaceId();
    Task<ImageSource> GetTicketImage(Guid ticketId);
    List<TicketsListViewModel> GetLatestTickets(TicketType type, int take);
    Task<List<TicketsListViewModel>> GetLatestTicketsAsync(TicketType type, int take);
    TicketsListViewModel? GetTicketDetails(Guid ticketId);
    Task<TicketsListViewModel?> GetTicketDetailsAsync(Guid ticketId);
    TicketsListViewModel? GetTicketDetails(long barcode);
    TicketsListViewModel? GetTicketDetailsByCardId(long cardUid);
    TicketsListViewModel? GetActiveTicketByCard(long cardUid);
    Guid? GetActiveLicensePlateTicketId(string licenseEnPlate);
    bool LicensePlateTicketIsExist(string licenseEnPlate);
    TServiceResponse<Guid> CreateTicket(CreateParkingTicketModel request, string? StartImage);
    List<TicketsListViewModel> GetTicketList(GetTicketListRequestModel request);
    Task<List<TicketsListViewModel>> GetTicketListAsync(GetTicketListRequestModel request);
    bool ExitRequest(Guid ticketId);
    void PaymentAmountCalculation(Guid ticketId);
    Task PaymentAmountCalculationAsync(Guid ticketId);
    short GetLicensePlateDiscountPercent(string licenseEnPlate);
    Task<short> GetLicensePlateDiscountPercentAsync(string licenseEnPlate);
    VehicleSegmentModel? GetVehicleSegmentById(int Id);
    string GetVehicleSegmentNameById(int Id);
    LicensePlateGroupModel? GetLicensePlateGroupByPlate(string licenseEnPlate);
    LicensePlateGroupModel? GetLicensePlateGroup(Guid id);
    List<LicensePlateListItemViewModel> GetLicensePlateGroupList();
    bool IsSeizedLicensePlate(string licenseEnPlate);
    List<SeizedLicensePlateModel> GetSeizedLicensePlatesList();
    Guid? GetGroupIdByEnLicensePlate(string enLicensePlate);
    bool SetTicketPaidInfo(TicketPaidInfoModel request);
    LicensePlateGroup? GetLicensePlateGroupById(Guid id);

    #region Cards
    decimal GetCardCreditAsync(long cardSerialNo);
    CardModel? GetCardInfo(long cardSerialNo);
    VehicleSegment? GetCardVehicleSegment(long cardSerialNo);
    LicensePlateGroup? GetCardLicensePlateGroup(long cardSerialNo);
    bool AddCardToGroup(long cardSerialNo, Guid groupId);
    bool AddCardToVehicleSegment(long cardSerialNo, int vehicleSegmentId);
    bool IsGuestCard(long cardSerialNo);
    bool CardActiveStatus(long cardSerialNo);
    bool CardExistStatus(long cardSerialNo);
    int GetCardDiscountPercent(long cardSerialNo);
    List<CardCreditHistoryModel> GetCardCreditHistory(long cardSerialNo, int page, int pageSize);
    List<TicketsListViewModel> GetCardUsageHistory(long cardSerialNo);
    bool AddCard(CardModel request);
    bool AddCardCreditHistory(CardCreditHistoryModel request);
    int GetCardsCount();
    bool IsCardInUse(long cardSerialNo);
    Guid? GetNotExitedTicketIdByCardSerialNo(long cardSerialNo);

    #endregion
    #region ExtraImages
    bool AddTicketExtraImage(Guid TicketId, string Image, string Name, bool ShowInPage);
    Task<bool> AddTicketExtraImageAsync(Guid TicketId, string Image, string Name, bool ShowInPage);
    bool DeleteTicketExtraImage(Guid TicketId, string Name);
    Task<bool> DeleteTicketExtraImageAsync(Guid TicketId, string Name);
    List<ParkingTicketExtraImageModel> GetTicketExtraImages(Guid TicketId, bool? ShowInBox);
    Task<List<ParkingTicketExtraImageModel>> GetTicketExtraImagesAsync(Guid TicketId, bool? ShowInBox);

    List<ParkingTicketExtraImageSourceModel> GetTicketExtraImageSources(Guid TicketId, bool? ShowInBox);
    Task<List<ParkingTicketExtraImageSourceModel>> GetTicketExtraImageSourcesAsync(Guid TicketId, bool? ShowInBox);


    #endregion
}
