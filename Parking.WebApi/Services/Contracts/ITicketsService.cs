using Parking.Domain.Entities.ParkingTicket;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Contracts;

public interface ITicketsService
{
    Task<ParkingTicket?> GetTicketByCardUidAsync(long cardUid);
    Task<CreateTicketResponse> CreateEntryTicketAsync(CreateEntryTicketRequest request);
    Task<TicketDetailsResponse?> GetTicketDetailsByCardUidAsync(long cardUid);
}