using Microsoft.EntityFrameworkCore;
using Parking.Domain.Contracts;
using Parking.Domain.Contracts.Base;
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
        IRepository<AddCardItem> addCardItems)
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
    }
    public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class
    {
        return await _context.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
    }
    public List<T> ExecuteRawQuery<T>(string sql, params object[] parameters) where T : class
    {
        return _context.Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToList();
    }
    //public void Dispose()
    //{
    //    _context.Dispose();
    //}
    //private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    //public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
    //{
    //    _contextFactory = contextFactory;

    //    _context = _contextFactory.CreateDbContext();
    //}

    //public IRepository<ApplicationRole> Roles => new Repository<ApplicationRole>(_context);
    //public IRepository<ApplicationUser> Users => new Repository<ApplicationUser>(_context);
    //public IRepository<ParkingLot> ParkingLots => new Repository<ParkingLot>(_context);
    //public IRepository<ParkingSection> ParkingSections => new Repository<ParkingSection>(_context);
    //public IRepository<ParkingSpace> ParkingSpaces => new Repository<ParkingSpace>(_context);
    //public IRepository<ParkingTicket> ParkingTickets => new Repository<ParkingTicket>(_context);
    //public IRepository<VehicleSegment> VehicleSegments => new Repository<VehicleSegment>(_context);
    //public IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices => new Repository<ParkingVehicleSegmentPrice>(_context);
    //public IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices => new Repository<ParkingVehicleSegmentVariablePrice>(_context);
    //public IRepository<LicensePlate> LicensePlates => new Repository<LicensePlate>(_context);
    //public IRepository<LicensePlateGroup> LicensePlateGroups => new Repository<LicensePlateGroup>(_context);
    //public IRepository<SeizedLicensePlate> SeizedLicensePlates => new Repository<SeizedLicensePlate>(_context);
    //public IRepository<Card> Cards => new Repository<Card>(_context);
    //public IRepository<CardCreditHistory> CardCreditHistories => new Repository<CardCreditHistory>(_context);
    //public IRepository<ParkingTicketImage> ParkingTicketImages => new Repository<ParkingTicketImage>(_context);

}


//public IRepository<ApplicationRole> Roles { get; private set; }
//public IRepository<ApplicationUser> Users { get; private set; }
//public IRepository<ParkingLot> ParkingLots { get; private set; }
//public IRepository<ParkingSection> ParkingSections { get; private set; }
//public IRepository<ParkingSpace> ParkingSpaces { get; private set; }
//public IRepository<ParkingTicket> ParkingTickets { get; private set; }
//public IRepository<VehicleSegment> VehicleSegments { get; private set; }
//public IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices { get; private set; }
//public IRepository<LicensePlate> LicensePlates { get; private set; }
//public IRepository<LicensePlateGroup> LicensePlateGroups { get; private set; }
//public IRepository<SeizedLicensePlate> SeizedLicensePlates { get; private set; }
//public IRepository<Card> Cards { get; private set; }
//public IRepository<CardCreditHistory> CardCreditHistories { get; private set; }
//public IRepository<ParkingTicketImage> ParkingTicketImages { get; private set; }
//public IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices { get; private set; }
//public UnitOfWork(
//    IRepository<ParkingVehicleSegmentVariablePrice> ParkingVehicleSegmentVariablePrices,
//    IRepository<ParkingVehicleSegmentPrice> ParkingVehicleSegmentPrices,
//    IRepository<ParkingTicketImage> ParkingTicketImages,
//    IRepository<CardCreditHistory> CardCreditHistories,
//    IRepository<Card> Cards,
//    IRepository<SeizedLicensePlate> SeizedLicensePlates,
//    IRepository<LicensePlateGroup> LicensePlateGroups,
//    IRepository<LicensePlate> LicensePlates,
//    IRepository<VehicleSegment> VehicleSegments,
//    IRepository<ParkingTicket> ParkingTickets,
//    IRepository<ParkingSpace> ParkingSpaces,
//    IRepository<ParkingSection> ParkingSections,
//    IRepository<ParkingLot> ParkingLots,
//    IRepository<ApplicationUser> Users,
//    IRepository<ApplicationRole> Roles)
//{

//    this.Roles = Roles;
//    this.Users = Users;
//    this.ParkingLots = ParkingLots;
//    this.ParkingSections = ParkingSections;
//    this.ParkingSpaces = ParkingSpaces;
//    this.ParkingTickets = ParkingTickets;
//    this.VehicleSegments = VehicleSegments;
//    this.ParkingVehicleSegmentPrices = ParkingVehicleSegmentPrices;
//    this.LicensePlates = LicensePlates;
//    this.LicensePlateGroups = LicensePlateGroups;
//    this.SeizedLicensePlates = SeizedLicensePlates;
//    this.Cards = Cards;
//    this.CardCreditHistories = CardCreditHistories;
//    this.ParkingTicketImages = ParkingTicketImages;
//    this.ParkingVehicleSegmentVariablePrices = ParkingVehicleSegmentVariablePrices;

//}



//}
//public async Task<int> CommitAsync(CancellationToken cancellationToken)
//{
//    return await _dbContext.SaveChangesAsync(cancellationToken);
//}
//public void Complete()
//{
//    _dbContext.SaveChanges();
//}
//public int Commit()
//{
//    return _dbContext.SaveChanges();
//}

