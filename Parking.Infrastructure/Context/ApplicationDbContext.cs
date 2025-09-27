using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;


namespace Parking.Infrastructure.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
    public DbSet<ParkingLot> ParkingLots { get; set; }
    public DbSet<ParkingSpace> ParkingSpaces { get; set; }
    public DbSet<ParkingSection> ParkingSections { get; set; }
    public DbSet<ParkingTicket> ParkingTickets { get; set; }
    public DbSet<VehicleSegment> VehicleSegments { get; set; }
    public DbSet<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrice { get; set; }
    public DbSet<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; set; }
    public DbSet<LicensePlateGroup> LicensePlateGroups { get; set; }
    public DbSet<LicensePlate> LicensePlates { get; set; }
    public DbSet<SeizedLicensePlate> SeizedLicensePlates { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardCreditHistory> CardCreditHistories { get; set; }
    public DbSet<ParkingTicketImage> ParkingTicketImages { get; set; }
    public DbSet<ParkingTicketExtraImage> ParkingTicketExtraImages { get; set; }
    public DbSet<TicketDescriptionItem> TicketDescriptionItems { get; set; }
    public DbSet<TicketQueueItem> TicketQueueItems { get; set; }
    public DbSet<TicketQueueResetPolicy> TicketQueueResetPolicies { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ParkingLot>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<VehicleSegment>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ParkingVehicleSegmentPrice>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ParkingVehicleSegmentVariablePrice>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<ApplicationUserRole>().HasNoKey();
        modelBuilder.Entity<TicketDescriptionItem>()
            .HasQueryFilter(t => !t.IsDeleted);

    }
}
