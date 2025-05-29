using Parking.App.Models.Dto.Parking.ParkingSection;
using Parking.App.Models.Dto.Parking.ParkingSpace;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;
using Parking.App.Models.GeneralServiceResponse;
using Parking.Domain.Entities.Vehicles;


namespace Parking.App.Services.Interfaces;

public interface ISynchronizationService
{
    TServiceResponse<bool> CheckToken(string userName, string password);
    Task<TServiceResponse<bool>> CheckTokenAsync(string userName, string password);
    TServiceResponse<bool> GetParkingLotDetailsFromServer();
    Task<TServiceResponse<bool>> GetParkingLotDetailsFromServerAsync();
    TServiceResponse<bool> GetParkingLotAccountsFromServer();
    Task<TServiceResponse<bool>> GetParkingLotAccountsFromServerAsync();
    TServiceResponse<bool> IsActiveParking();
    Task<TServiceResponse<bool>> IsActiveParkingAsync();
    TServiceResponse<bool> SendUnSyncedTicketToServer();
    Task<TServiceResponse<bool>> SendUnSyncedTicketToServerAsync();
    void SyncTicketImage();
    void SyncTicketExitImage();
    Task SyncTicketImageAsync();
    Task SyncTicketExtraImagesAsync();
    TServiceResponse<bool> ReceiveLicensePlateGroupFromServer();
    Task<TServiceResponse<bool>> ReceiveLicensePlateGroupFromServerAsync();
    TServiceResponse<bool> ReceiveSeizedLicensePlateFromServer();
    Task<TServiceResponse<bool>> ReceiveSeizedLicensePlateFromServerAsync();
    TServiceResponse<bool> ReceiveVehicleSegmentsListFromServer();
    Task<TServiceResponse<bool>> ReceiveVehicleSegmentsListFromServerAsync();
    List<LicensePlateGroup> GetLicensePlateGroups();
    Task<List<LicensePlateGroup>> GetLicensePlateGroupsAsync();
    TServiceResponse<bool> CreateVehicleSegmentsPrice(List<ParkingVehicleSegmentPriceModel> request);
    Task<bool> ServerConnectiviyCheckAsync();
}

