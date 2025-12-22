using Parking.WebApi.Requests;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Services.Contracts;

public interface ITicketsService
{
    Task<CreateTicketResponse> CreateEntryTicketAsync(CreateEntryTicketRequest request);
}