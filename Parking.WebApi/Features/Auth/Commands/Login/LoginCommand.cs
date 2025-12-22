using MediatR;
using Parking.WebApi.Application.Common.Models;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;
