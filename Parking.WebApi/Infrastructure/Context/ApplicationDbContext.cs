using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;

namespace Parking.WebApi.Infrastructure.Context;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<ParkingLot> ParkingLots { get; set; } = null!;
    public DbSet<ParkingSpace> ParkingSpaces { get; set; } = null!;
    public DbSet<ParkingSection> ParkingSections { get; set; } = null!;
    public DbSet<ParkingTicket> ParkingTickets { get; set; } = null!;
    public DbSet<VehicleSegment> VehicleSegments { get; set; } = null!;
    public DbSet<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices { get; set; } = null!;
    public DbSet<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; set; } = null!;
    public DbSet<LicensePlateGroup> LicensePlateGroups { get; set; } = null!;
    public DbSet<LicensePlate> LicensePlates { get; set; } = null!;
    public DbSet<SeizedLicensePlate> SeizedLicensePlates { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<CardCreditHistory> CardCreditHistories { get; set; } = null!;
    public DbSet<ParkingTicketImage> ParkingTicketImages { get; set; } = null!;
    public DbSet<ParkingTicketExtraImage> ParkingTicketExtraImages { get; set; } = null!;
    public DbSet<TicketDescriptionItem> TicketDescriptionItems { get; set; } = null!;
    public DbSet<TicketQueueItem> TicketQueueItems { get; set; } = null!;
    public DbSet<TicketQueueResetPolicy> TicketQueueResetPolicies { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;
    public DbSet<AddCardItem> AddCardItems { get; set; } = null!;
}