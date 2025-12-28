using MediatR;
using Microsoft.AspNetCore.Identity;
using Parking.Domain.Entities.User;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
            return Result<LoginResponse>.Failure("نام کاربری یا رمز عبور اشتباه است");

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
            return Result<LoginResponse>.Failure("نام کاربری یا رمز عبور اشتباه است");

        var token = await jwtService.GenerateTokenAsync(user);
        var response = new LoginResponse
        {
            Token = token,
            FirstName = user.Firstname,
            LastName = user.Lastname
        };

        return Result<LoginResponse>.Success(response, "ورود موفق");
    }
}
