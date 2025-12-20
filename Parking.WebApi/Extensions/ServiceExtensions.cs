using Microsoft.AspNetCore.Identity;
using Parking.Domain.Entities.User;
using Parking.WebApi.Services.Contracts;
using Parking.WebApi.Services.Implementations;

namespace Parking.WebApi.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<UserManager<ApplicationUser>>();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVehicleSegmentsService, VehicleSegmentsService>();
    }
}