using Parking.App.Models.Dto.Card;
using Parking.App.Models.Dto.Parking.ParkingLot;
using Parking.App.Models.Dto.Parking.ParkingSection;
using Parking.App.Models.Dto.Parking.ParkingSpace;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.GeneralServiceResponse;
using Parking.Domain.Entities.Vehicles;

namespace Parking.App.Services.Interfaces;

public interface IParkingService
{
    TServiceResponse<ParkingLotModel> GetParkingLotDetails();
    Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceList();
    Task<List<VehicleSegmentPriceListItemModel>> GetVehicleSegmentPriceListAsync();
    List<VehicleSegmentModel> GetVehicleSegments();
    List<ParkingSectionModel> GetSections();
    List<ParkingSpaceModel> GetSpaces();
    int? GetFreeSpacesCount();
    int? GetSpacesCount();
    int? GetParkingLotCapacity();
    (Guid? SpaceId, Guid? SectionId) GetOneFreeSpaceId();
    Task<(ImageSource? StartImage, ImageSource? ExitImage)> GetTicketImages(Guid ticketId);
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
    (List<TicketsListViewModel> Data, int TotalCount) GetTicketList(GetTicketListRequestModel request);
    Task<(List<TicketsListViewModel> Data, int TotalCount)> GetTicketListAsync(GetTicketListRequestModel request);
    Task<(List<TicketsListViewModel> Data, int TotalCount)> GetTicketListReportAsync(GetTicketListRequestModel request);
    bool ExitRequest(Guid ticketId);
    void PaymentAmountCalculation(Guid ticketId);
    Task PaymentAmountCalculationAsync(Guid ticketId);
    short GetLicensePlateDiscountPercent(string licenseEnPlate);
    Task<short> GetLicensePlateDiscountPercentAsync(string licenseEnPlate);
    VehicleSegmentModel? GetVehicleSegmentById(int Id);
    string GetVehicleSegmentNameById(int Id);
    LicensePlateGroupModel? GetLicensePlateGroupByPlate(string licenseEnPlate);
    LicensePlateGroupModel? GetLicensePlateGroup(Guid id);
    Task<List<LicensePlateGroupModel>> GetLicensePlateList();
    Task<bool> AddLicensePlateGroup(LicensePlateGroup licensePlate);
    (List<LicensePlateListItemViewModel> Data, int TotalCount) GetLicensePlateGroupList(string? EnLicensePlate, int Page, int PageSize);
    bool IsSeizedLicensePlate(string licenseEnPlate);
    List<SeizedLicensePlateModel> GetSeizedLicensePlatesList();
    Guid? GetGroupIdByEnLicensePlate(string enLicensePlate);
    bool SetTicketPaidInfo(TicketPaidInfoModel request);
    LicensePlateGroup? GetLicensePlateGroupById(Guid id);
    List<string?> GetEntryRegistrars();
    List<string?> GetExitRegistrars();

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
    int GetDiscountedCardsCount();
    bool IsCardInUse(long cardSerialNo);
    Guid? GetNotExitedTicketIdByCardSerialNo(long cardSerialNo);
    (bool Result, string ResultMSG) CreateAddCardHistory(AddCardItemModel request);
    (List<AddCardItemModel> Result, int ResultCount, string ResultMSG) SearchInCardHistory(string? FullName, long? CardUid, string? EnLicensePlate, int? PercentDiscount, DateTime? StartCreateDate, DateTime? EndCreateDate, string? Description, int Page, int PageSize);

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
    Guid? GetTicketIdByBarcode(long barcode);

    #endregion


    #region TicketDescription
    List<TicketDescriptionItemModel> GetAllTicketDescriptionItems();
    TicketDescriptionItemModel? GetTicketDescriptionItemById(int id);
    bool AddTicketDescriptionItem(TicketDescriptionItemModel request);
    bool UpdateTicketDescriptionItem(TicketDescriptionItemModel request);
    Task<bool> ResetTicketDescriptionInterval(int Id);
    bool ChangeTicketDescriptionItemQueueStatus(int id, bool status);
    (bool IsSuccess, bool IsExsist) DeleteTicketDescriptionItem(int id);
    Task<TicketSummaryReportModel> GetSummaryReport(GetTicketListRequestModel request);
    Task<(bool Exists, bool IsSuccess)> AddSeizedVehicleAsync(string plate, string reason);
    Task<bool> DeleteSeizedVehicleAsync(Guid Id);

    #endregion
}
