using MediatR;
using Parking.WebApi.Application.Common.Exceptions;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler(
    ITicketsService ticketsService)
    : IRequestHandler<CreateTicketCommand, Result<CreateTicketResponse>>
{
    public async Task<Result<CreateTicketResponse>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entryTicketRequest = new CreateEntryTicketRequest
            {
                EnLicensePlate = request.EnLicensePlate,
                PlateType = request.PlateType,
                VehicleSegmentId = request.VehicleSegmentId,
                DeviceName = request.DeviceName,
                Base64Images = request.Base64Images,
                CardUid = request.CardUid
            };

            var createTicketResponse = await ticketsService.CreateEntryTicketAsync(entryTicketRequest);

            return createTicketResponse is null
                ? Result<CreateTicketResponse>.Failure("صدور بلیط با خطا مواجه شد", "کارت برای خودروی دیگری ثبت شده است")
                : Result<CreateTicketResponse>.Success(createTicketResponse, "بلیط با موفقیت ثبت شد");
        }
        catch (CustomNotFoundException ex)
        {
            return Result<CreateTicketResponse>.Failure(ex.Message, "یافت نشد");
        }
    }
}
