using System.Security.Claims;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return userId is null ? Guid.Empty : Guid.Parse(userId);
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}