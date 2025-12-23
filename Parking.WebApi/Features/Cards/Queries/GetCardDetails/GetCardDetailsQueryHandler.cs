using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public class GetCardDetailsQueryHandler(
    ICardService cardService, 
    ITicketsService ticketsService) : IRequestHandler<GetCardDetailsQuery, Result<PlateAndTariffResponse>>
{
    public async Task<Result<PlateAndTariffResponse>> Handle(GetCardDetailsQuery request, CancellationToken cancellationToken)
    {
        var card = await cardService.GetCardByUidAsync(request.CardUid);

        if (card is null)
            return Result<PlateAndTariffResponse>.Failure("کارت با این شناسه وجود ندارد", "کارت یافت نشد");

        if (!card.IsActive)
            return Result<PlateAndTariffResponse>.Failure("این کارت فعال نشده است و نمی‌توان از آن استفاده کرد", "کارت غیرفعال است");

        var ticket = await ticketsService.GetTicketByCardUidAsync((long)card.CardSerialNo!);
        if (ticket is null)
            return Result<PlateAndTariffResponse>.Failure("برای این کارت بلیطی صادر نشده است", "بلیط یافت نشد");

        var response = new PlateAndTariffResponse
        {
            FaLicensePlate = ticket.LicensePlate,
            EnLicencePlate = ticket.EnLicensePlate,
            Tariff = ticket.VehicleManufacturerName
        };

        return Result<PlateAndTariffResponse>.Success(response, "جزئیات کارت با موفقیت دریافت شد");
    }
}
