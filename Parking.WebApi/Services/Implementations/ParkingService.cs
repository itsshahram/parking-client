using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Infrastructure.Context;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

// public class ParkingService(
//     ApplicationDbContext context) : IParkingService
// {
//     public Task<ParkingTicket?> GetTicketByCardUidAsync(decimal? cardUid)
//     {
//         var ticket = context
//             .ParkingTickets
//             .FirstOrDefaultAsync(t => t.CardUid == cardUid);
//         
//         return ticket;
//     }
// }