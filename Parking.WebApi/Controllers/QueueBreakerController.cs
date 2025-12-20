using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Parking.Domain.Entities.User;
using Parking.WebApi.Requests;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueBreakerController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IVehicleSegmentsService vehicleSegmentsService)
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
            FirstName = user.Firstname!,
            LastName = user.Lastname!
        };

        return Ok(ApiResponse<LoginResponse>.Success(response, "ورود موفق"));
    }
    
    
    [Authorize]
    [HttpGet("tariffs")]
    public async Task<IActionResult> GetTariffs()
    {
        var tariffs = await vehicleSegmentsService.GetAllTariffsAsync();
        return Ok(ApiResponse<List<VehicleSegmentResponse>>.Success(tariffs, "تعرفه ها با موفقیت دریافت شد"));
    }
}