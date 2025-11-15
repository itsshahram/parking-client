using Microsoft.EntityFrameworkCore;
using Parking.Domain.Contracts;
using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;
using Parking.Infrastructure.Context;

namespace Parking.Infrastructure.Uow;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public IRepository<ApplicationRole> Roles { get; }
    public IRepository<ApplicationUser> Users { get; }
    public IRepository<ParkingLot> ParkingLots { get; }
    public IRepository<ParkingSection> ParkingSections { get; }
    public IRepository<ParkingSpace> ParkingSpaces { get; }
    public IRepository<ParkingTicket> ParkingTickets { get; }
    public IRepository<VehicleSegment> VehicleSegments { get; }
    public IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices { get; }
    public IRepository<LicensePlate> LicensePlates { get; }
    public IRepository<LicensePlateGroup> LicensePlateGroups { get; }
    public IRepository<SeizedLicensePlate> SeizedLicensePlates { get; }
    public IRepository<Card> Cards { get; }
    public IRepository<CardCreditHistory> CardCreditHistories { get; }
    public IRepository<ParkingTicketImage> ParkingTicketImages { get; }
    public IRepository<ParkingTicketExtraImage> ParkingTicketExtraImages { get; }
    public IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; }
    public IRepository<AddCardItem> AddCardItems { get; }
    public IRepository<TicketDescriptionItem> TicketDescriptionItems { get; }
    public IRepository<TicketQueueItem> TicketQueueItems { get; }
    public IRepository<TicketQueueResetPolicy> TicketQueueResetPolicies { get; }
    public IRepository<RolePermission> RolePermissions { get; }
    public IRepository<Permission> Permissions { get; }
    public IRepository<UserPermission> UserPermissions { get; }

    public UnitOfWork(
        IDbContextFactory<ApplicationDbContext> factory,
        IRepository<ApplicationRole> roles,
        IRepository<ApplicationUser> users,
        IRepository<ParkingLot> parkingLots,
        IRepository<ParkingSection> parkingSections,
        IRepository<ParkingSpace> parkingSpaces,
        IRepository<ParkingTicket> parkingTickets,
        IRepository<VehicleSegment> vehicleSegments,
        IRepository<ParkingVehicleSegmentPrice> parkingVehicleSegmentPrices,
        IRepository<LicensePlate> licensePlates,
        IRepository<LicensePlateGroup> licensePlateGroups,
        IRepository<SeizedLicensePlate> seizedLicensePlates,
        IRepository<Card> cards,
        IRepository<CardCreditHistory> cardCreditHistories,
        IRepository<ParkingTicketImage> parkingTicketImages,
        IRepository<ParkingTicketExtraImage> parkingTicketExtraImages,
        IRepository<ParkingVehicleSegmentVariablePrice> parkingVehicleSegmentVariablePrices,
        IRepository<AddCardItem> addCardItems,
        IRepository<TicketDescriptionItem> ticketDescriptionItems,
        IRepository<TicketQueueItem> ticketQueueItems,
        IRepository<TicketQueueResetPolicy> ticketQueueResetPolicies,
        IRepository<RolePermission> rolePermissions,
        IRepository<Permission> permissions,
        IRepository<UserPermission> userPermissions)
    {
        _factory = factory;

        Roles = roles;
        Users = users;
        ParkingLots = parkingLots;
        ParkingSections = parkingSections;
        ParkingSpaces = parkingSpaces;
        ParkingTickets = parkingTickets;
        VehicleSegments = vehicleSegments;
        ParkingVehicleSegmentPrices = parkingVehicleSegmentPrices;
        LicensePlates = licensePlates;
        LicensePlateGroups = licensePlateGroups;
        SeizedLicensePlates = seizedLicensePlates;
        Cards = cards;
        CardCreditHistories = cardCreditHistories;
        ParkingTicketImages = parkingTicketImages;
        ParkingTicketExtraImages = parkingTicketExtraImages;
        ParkingVehicleSegmentVariablePrices = parkingVehicleSegmentVariablePrices;
        AddCardItems = addCardItems;
        TicketDescriptionItems = ticketDescriptionItems;
        TicketQueueItems = ticketQueueItems;
        TicketQueueResetPolicies = ticketQueueResetPolicies;
        RolePermissions = rolePermissions;
        Permissions = permissions;
        UserPermissions = userPermissions;
    }


    public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters)
        where T : class
    {
        using var db = _factory.CreateDbContext();
        return await db.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
    }

    public List<T> ExecuteRawQuery<T>(string sql, params object[] parameters)
        where T : class
    {
        using var db = _factory.CreateDbContext();
        return db.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToList();
    }
}
