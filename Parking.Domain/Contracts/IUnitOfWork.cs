using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;

namespace Parking.Domain.Contracts;
public interface IUnitOfWork
{
    IRepository<ApplicationRole> Roles { get; }
    IRepository<ApplicationUser> Users { get; }
    IRepository<ParkingLot> ParkingLots { get; }
    IRepository<ParkingSection> ParkingSections { get; }
    IRepository<ParkingSpace> ParkingSpaces { get; }
    IRepository<ParkingTicket> ParkingTickets { get; }
    IRepository<VehicleSegment> VehicleSegments { get; }
    IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices { get; }
    IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; }
    IRepository<LicensePlate> LicensePlates { get; }
    IRepository<LicensePlateGroup> LicensePlateGroups { get; }
    IRepository<SeizedLicensePlate> SeizedLicensePlates { get; }
    IRepository<Card> Cards { get; }
    IRepository<CardCreditHistory> CardCreditHistories { get; }
    IRepository<ParkingTicketImage> ParkingTicketImages { get; }
    IRepository<ParkingTicketExtraImage> ParkingTicketExtraImages { get; }
    IRepository<AddCardItem> AddCardItems { get; }
    IRepository<TicketDescriptionItem> TicketDescriptionItems { get; }
    IRepository<TicketQueueItem> TicketQueueItems { get; }
    IRepository<TicketQueueResetPolicy> TicketQueueResetPolicies { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<UserPermission> UserPermissions { get; }


    Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class;
    List<T> ExecuteRawQuery<T>(string sql, params object[] parameters) where T : class;
}
