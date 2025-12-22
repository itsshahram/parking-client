namespace Parking.WebApi.Services.Contracts;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}