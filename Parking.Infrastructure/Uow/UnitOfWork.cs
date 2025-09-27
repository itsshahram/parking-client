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
    private readonly ApplicationDbContext _context;
    public IRepository<ApplicationRole> Roles { get; private set; }
    public IRepository<ApplicationUser> Users { get; private set; }
    public IRepository<ParkingLot> ParkingLots { get; private set; }
    public IRepository<ParkingSection> ParkingSections { get; private set; }
    public IRepository<ParkingSpace> ParkingSpaces { get; private set; }
    public IRepository<ParkingTicket> ParkingTickets { get; private set; }
    public IRepository<VehicleSegment> VehicleSegments { get; private set; }
    public IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices { get; private set; }
    public IRepository<LicensePlate> LicensePlates { get; private set; }
    public IRepository<LicensePlateGroup> LicensePlateGroups { get; private set; }
    public IRepository<SeizedLicensePlate> SeizedLicensePlates { get; private set; }
    public IRepository<Card> Cards { get; private set; }
    public IRepository<CardCreditHistory> CardCreditHistories { get; private set; }
    public IRepository<ParkingTicketImage> ParkingTicketImages { get; private set; }
    public IRepository<ParkingTicketExtraImage> ParkingTicketExtraImages { get; private set; }
    public IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; private set; }
    public IRepository<AddCardItem> AddCardItems { get; private set; }
    public IRepository<TicketDescriptionItem> TicketDescriptionItems { get; private set; }
    public IRepository<TicketQueueItem> TicketQueueItems { get; private set; }
    public IRepository<TicketQueueResetPolicy> TicketQueueResetPolicies { get; private set; }
    public IRepository<RolePermission> RolePermissions { get; private set; }
    public IRepository<Permission> Permissions { get; private set; }
    public IRepository<UserPermission> UserPermissions { get; private set; }

    public UnitOfWork(ApplicationDbContext context,
        IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices,
        IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices,
        IRepository<ParkingTicketImage> ParkingTicketImages,
        IRepository<CardCreditHistory> CardCreditHistories,
        IRepository<Card> Cards,
        IRepository<SeizedLicensePlate> SeizedLicensePlates,
        IRepository<LicensePlateGroup> LicensePlateGroups,
        IRepository<LicensePlate> LicensePlates,
        IRepository<VehicleSegment> VehicleSegments,
        IRepository<ParkingTicket> ParkingTickets,
        IRepository<ParkingSpace> ParkingSpaces,
        IRepository<ParkingSection> ParkingSections,
        IRepository<ParkingLot> ParkingLots,
        IRepository<ApplicationUser> Users,
        IRepository<ApplicationRole> Roles,
        IRepository<ParkingTicketExtraImage> ParkingTicketExtraImages,
        IRepository<AddCardItem> addCardItems,
        IRepository<TicketDescriptionItem> TicketDescriptionItems,
        IRepository<TicketQueueItem> TicketQueueItems,
        IRepository<TicketQueueResetPolicy> TicketQueueResetPolicies,
        IRepository<RolePermission> rolePermissions,
        IRepository<Permission> permissions,
        IRepository<UserPermission> userPermissions)
    {

        this.Roles = Roles;
        this.Users = Users;
        this.ParkingLots = ParkingLots;
        this.ParkingSections = ParkingSections;
        this.ParkingSpaces = ParkingSpaces;
        this.ParkingTickets = ParkingTickets;
        this.VehicleSegments = VehicleSegments;
        this.ParkingVehicleSegmentPrices = ParkingVehicleSegmentPrices;
        this.LicensePlates = LicensePlates;
        this.LicensePlateGroups = LicensePlateGroups;
        this.SeizedLicensePlates = SeizedLicensePlates;
        this.Cards = Cards;
        this.CardCreditHistories = CardCreditHistories;
        this.ParkingTicketImages = ParkingTicketImages;
        this.ParkingVehicleSegmentVariablePrices = ParkingVehicleSegmentVariablePrices;
        this.ParkingTicketExtraImages = ParkingTicketExtraImages;
        _context = context;
        AddCardItems = addCardItems;
        this.TicketDescriptionItems = TicketDescriptionItems;
        this.TicketQueueItems = TicketQueueItems;
        this.TicketQueueResetPolicies = TicketQueueResetPolicies;
        RolePermissions = rolePermissions;
        Permissions = permissions;
        UserPermissions = userPermissions;
    }
    public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class
    {
        return await _context.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
    }
    public List<T> ExecuteRawQuery<T>(string sql, params object[] parameters) where T : class
    {
        return _context.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToList();
    }
}

