using Microsoft.AspNetCore.Identity;
using Parking.Domain.Entities.User;

namespace Parking.WebApi.Services.Contracts;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}