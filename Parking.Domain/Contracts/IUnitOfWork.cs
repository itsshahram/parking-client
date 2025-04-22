using Parking.Domain.Contracts.Base;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.ParkingTicket;
using Parking.Domain.Entities.User;
using Parking.Domain.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Contracts;
public interface IUnitOfWork  //:IDisposable
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
    Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class;
    List<T> ExecuteRawQuery<T>(string sql, params object[] parameters) where T : class;

    //Task<int> CommitAsync(CancellationToken cancellationToken);
    //void Complete();
    //int Commit();
}
