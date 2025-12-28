using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Parking.Domain.Entities.User;
using Parking.WebApi.Services.Contracts;

namespace Parking.WebApi.Services.Implementations;

public class JwtService(
    IConfiguration configuration, 
    UserManager<ApplicationUser> userManager) : IJwtService
{
    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

        // Validate JWT configuration
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var jwtExpiryMinutes = configuration["Jwt:ExpiryMinutes"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("تنظیمات JWT:Key در فایل پیکربندی یافت نشد");

        if (string.IsNullOrWhiteSpace(jwtIssuer))
            throw new InvalidOperationException("تنظیمات JWT:Issuer در فایل پیکربندی یافت نشد");

        if (string.IsNullOrWhiteSpace(jwtAudience))
            throw new InvalidOperationException("تنظیمات JWT:Audience در فایل پیکربندی یافت نشد");

        if (string.IsNullOrWhiteSpace(jwtExpiryMinutes) || !double.TryParse(jwtExpiryMinutes, out var expiryMinutes) || expiryMinutes <= 0)
            throw new InvalidOperationException("تنظیمات JWT:ExpiryMinutes نامعتبر است");

        if (string.IsNullOrWhiteSpace(user.UserName))
            throw new InvalidOperationException("نام کاربری نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new InvalidOperationException("ایمیل کاربر نمی‌تواند خالی باشد");

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}