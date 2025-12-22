using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Parking.Domain.Entities.User;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Controllers;

[ApiController]
[Route("api/queue-breaker")]
public class QueueBreakerController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IVehicleSegmentsService vehicleSegmentsService,
    ICardService cardService,
    IParkingService parkingService,
    ITicketsService ticketsService)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await userManager.FindByNameAsync(loginRequest.Username);

        if (user == null)
            return Unauthorized(ApiResponse<object>.Fail("نام کاربری یا رمز عبور اشتباه است", statusCode: 401));

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
            return Unauthorized(ApiResponse<object>.Fail("نام کاربری یا رمز عبور اشتباه است", statusCode: 401));

        var token = await jwtService.GenerateTokenAsync(user);
        var response = new LoginResponse
        {
            Token = token,
            FirstName = user.Firstname,
            LastName = user.Lastname
        };

        return Ok(ApiResponse<LoginResponse>.Success(response, "ورود موفق"));
    }
    
    [Authorize]
    [HttpPost("create-ticket")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateEntryTicketRequest createEntryTicketRequest)
    {
        var ticketResponse = await ticketsService.CreateEntryTicketAsync(createEntryTicketRequest);
        
        if (ticketResponse.TicketId == Guid.Empty && string.IsNullOrEmpty(ticketResponse.BarcodeId))
            return BadRequest(ApiResponse<object>.Fail("صدور بلیط با خطا مواجه شد", "کارت در حال استفاده است", 400));
        
        return Ok(ApiResponse<CreateTicketResponse>.Success(ticketResponse, "بلیط پارکینگ با موفقیت ثبت شد"));
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
        var tariffs = await vehicleSegmentsService.GetAllTariffsAsync();
        return Ok(ApiResponse<List<VehicleSegmentResponse>>.Success(tariffs, "تعرفه ها با موفقیت دریافت شد"));
    }

    [Authorize]
    [HttpGet("card-details/{cardUid:long}")]
    public async Task<IActionResult> GetCardDetails([FromRoute] long cardUid)
    {
        var card = await cardService.GetCardByUidAsync(cardUid);

        if (card is null)
            return NotFound(ApiResponse<object>.Fail("کارت یافت نشد", "کارت با این شناسه وجود ندارد", 404));

        if (!card.IsActive)
            return BadRequest(ApiResponse<object>.Fail("کارت غیرفعال است", "این کارت فعال نشده است و نمی‌توان از آن استفاده کرد", 400));

        var ticket = await parkingService.GetTicketByCardUidAsync(card.CardSerialNo);
        if (ticket is null)
            return NotFound(ApiResponse<object>.Fail("بلیط یافت نشد", "برای این کارت بلیطی صادر نشده است", 404));

        var response = new PlateAndTariffResponse
        {
            FaLicensePlate = ticket.LicensePlate,
            EnLicencePlate = ticket.EnLicensePlate,
            Tariff = ticket.VehicleManufacturerName
        };

        return Ok(ApiResponse<PlateAndTariffResponse>.Success(response, "جزئیات کارت با موفقیت دریافت شد"));
    }
}