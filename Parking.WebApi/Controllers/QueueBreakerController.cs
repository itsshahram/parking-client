using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.WebApi.Features.Auth.Commands.Login;
using Parking.WebApi.Features.Cards.Queries.GetCardDetails;
using Parking.WebApi.Features.Tariffs.Queries.GetTariffs;
using Parking.WebApi.Features.Tickets.Commands.CreateTicket;
using Parking.WebApi.Features.Tickets.Commands.UpdateTicketPayment;
using Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByBarcodeId;
using Parking.WebApi.Features.Tickets.Queries.GetTicketDetailsByCardUid;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Controllers;

[ApiController]
[Route("api/queue-breaker")]
public class QueueBreakerController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var command = new LoginCommand(loginRequest.Username, loginRequest.Password);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.Fail(result.Errors, result.Message, 401));

        return Ok(ApiResponse<LoginResponse>.Success(result.Data!, result.Message));
    }
    
    [Authorize]
    [HttpPost("create-ticket")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateEntryTicketRequest createEntryTicketRequest)
    {
        var command = new CreateTicketCommand(
            createEntryTicketRequest.EnLicensePlate,
            createEntryTicketRequest.PlateType,
            createEntryTicketRequest.VehicleSegmentId,
            createEntryTicketRequest.DeviceName,
            createEntryTicketRequest.Base64Images,
            createEntryTicketRequest.CardUid);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Errors, result.Message, 400));

        return Ok(ApiResponse<CreateTicketResponse>.Success(result.Data!, result.Message));
    }
    
    [Authorize]
    [HttpPut("ticket-payment")]
    public async Task<IActionResult> TicketPayment([FromBody] PaymentRequest paymentRequest)
    {
        var command = new UpdateTicketPaymentCommand(
            paymentRequest.PaidType,
            paymentRequest.Base64Images,
            paymentRequest.TicketId,
            paymentRequest.Amount,
            paymentRequest.ExitGate,
            paymentRequest.DeviceId,
            paymentRequest.PaidDate,
            paymentRequest.PaidCreditCard,
            paymentRequest.MerchantNumber,
            paymentRequest.Rrn,
            paymentRequest.TraceNo,
            paymentRequest.RefId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message, 400));

        return Ok(ApiResponse.Success(result.Message));
    }

    [HttpGet("get-plate-types")]
    public IActionResult GetPlateTypes()
    {
        var plateTypes = Enum.GetValues(typeof(PlateType))
            .Cast<PlateType>()
            .Select(p => new
            {
                value = (int)p,
                displayName = p switch
                {
                    PlateType.NormalPersianPlates => "پلاک معمولی خودرو",
                    PlateType.Motorcycle => "موتورسیکلت",
                    PlateType.FreeZone => "منطقه آزاد",
                    PlateType.Foreign => "پلاک خارجی",
                    _ => p.ToString()
                }
            })
            .OrderBy(x => x.value);

        return Ok(plateTypes);
    }

    [Authorize]
    [HttpGet("get-tariffs")]
    public async Task<IActionResult> GetTariffs()
    {
        var query = new GetTariffsQuery();
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Errors, result.Message, 400));

        return Ok(ApiResponse<List<VehicleSegmentResponse>>.Success(result.Data!, result.Message));
    }

    [Authorize]
    [HttpGet("card-details/{cardUid:long}")]
    public async Task<IActionResult> GetCardDetails([FromRoute] long cardUid)
    {
        var query = new GetCardDetailsQuery(cardUid);
        var result = await mediator.Send(query);

        if (result.IsSuccess) 
            return Ok(ApiResponse<PlateAndTariffResponse>.Success(result.Data!, result.Message));
        
        var statusCode = result.Message?.Contains("یافت نشد") == true ? 404 : 400;
        
        return StatusCode(statusCode, ApiResponse<object>.Fail(result.Errors, result.Message, statusCode));
    }

    [Authorize]
    [HttpGet("get-ticket-details/{cardUid:long}")]
    public async Task<IActionResult> GetTicketDetailsByCardUid([FromRoute] long cardUid)
    {
        var query = new GetTicketDetailsByCardUidQuery(cardUid);
        var result = await mediator.Send(query);

        if (result.IsSuccess) 
            return Ok(ApiResponse<TicketDetailsResponse>.Success(result.Data!, result.Message));
        
        var statusCode = result.Message?.Contains("یافت نشد") == true ? 404 : 400;
        
        return StatusCode(statusCode, ApiResponse<object>.Fail(result.Errors, result.Message, statusCode));
    }
    
    [Authorize]
    [HttpGet("get-ticket-details/barcodeId/{barcodeId:long}")]
    public async Task<IActionResult> GetTicketDetailsByBarcodeId([FromRoute] long barcodeId)
    {
        var query = new GetTicketDetailsByBarcodeIdQuery(barcodeId);
        var result = await mediator.Send(query);

        if (result.IsSuccess) 
            return Ok(ApiResponse<TicketDetailsResponse>.Success(result.Data!, result.Message));
        
        var statusCode = result.Message?.Contains("یافت نشد") == true ? 404 : 400;
        
        return StatusCode(statusCode, ApiResponse<object>.Fail(result.Errors, result.Message, statusCode));
    }
}