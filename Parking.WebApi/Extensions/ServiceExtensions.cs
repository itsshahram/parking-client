using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Parking.Domain.Entities.User;
using Parking.WebApi.Application.Abstractions.EntityRepositories;
using Parking.WebApi.Application.Abstractions.UnitOfWork;
using Parking.WebApi.Application.Common.Behaviors;
using Parking.WebApi.Infrastructure.Implementation;
using Parking.WebApi.Services.Contracts;
using Parking.WebApi.Services.Implementations;

namespace Parking.WebApi.Extensions;

public static class ServiceExtensions
{
    public static void AddToDi(this IServiceCollection services)
    {
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ILicensePlateRepository, LicensePlateRepository>();
        services.AddScoped<ILicensePlateGroupRepository, LicensePlateGroupRepository>();
        services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IVehicleSegmentRepository, VehicleSegmentRepository>();
        services.AddScoped<ITicketExtraImageRepository, TicketExtraImageRepository>();
        
        services.AddScoped<UserManager<ApplicationUser>>();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IVehicleSegmentsService, VehicleSegmentsService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IParkingService, ParkingService>();
        services.AddScoped<ITicketsService, TicketsService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}