using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public class GetCardDetailsQueryHandler : IRequestHandler<GetCardDetailsQuery, Result<PlateAndTariffResponse>>
{
    private readonly ICardService _cardService;
    private readonly IParkingService _parkingService;

    public GetCardDetailsQueryHandler(ICardService cardService, IParkingService parkingService)
    {
        _cardService = cardService;
        _parkingService = parkingService;
    }

    public async Task<Result<PlateAndTariffResponse>> Handle(GetCardDetailsQuery request, CancellationToken cancellationToken)
    {
        var card = await _cardService.GetCardByUidAsync(request.CardUid);

        if (card is null)
            return Result<PlateAndTariffResponse>.Failure("کارت با این شناسه وجود ندارد", "کارت یافت نشد");

        if (!card.IsActive)
            return Result<PlateAndTariffResponse>.Failure("این کارت فعال نشده است و نمی‌توان از آن استفاده کرد", "کارت غیرفعال است");

        var ticket = await _parkingService.GetTicketByCardUidAsync(card.CardSerialNo);
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
