using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class CardService(
    ICardRepository cardRepository,
    ITicketsService ticketsService) : ICardService
{
    public async Task<PlateAndTariffResponse> GetCardByUidAsync(long cardUid)
    {
        var card = await cardRepository.GetCardByCardSerialNoOrBarcodeIdAsync(cardUid);

        if (card is null)
            throw new CustomNotFoundException("کارت با این شناسه وجود ندارد");

        if (!card.IsActive)
            throw new InActiveCardException("کارت غیرفعال است و نمی توان از آن استفاده کرد");
        
        var ticket = await ticketsService.GetTicketByCardUidAsync((long)card.CardSerialNo!);
        
        if (ticket is null)
            throw new CustomNotFoundException("برای این کارت بلیطی صادر نشده است");
        
        var response = new PlateAndTariffResponse
        {
            FaLicensePlate = ticket.LicensePlate,
            EnLicencePlate = ticket.EnLicensePlate,
            Tariff = ticket.VehicleManufacturerName
        };
        
        return response;
    }
}