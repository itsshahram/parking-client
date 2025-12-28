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
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("کاربر احراز هویت نشده است");

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن یافت نشد");

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new InvalidOperationException("شناسه کاربر نامعتبر است");

            return parsedUserId;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}