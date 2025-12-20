using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Parking.WebApi.Responses;

namespace Parking.WebApi.Extensions;

public static class AuthenticationExtensions
{
    public static void AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };

            // هندل کردن خطاهای احراز هویت
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    
                    var response = context.Exception is SecurityTokenExpiredException
                        ? ApiResponse<object>.Fail("توکن منقضی شده است. لطفاً دوباره لاگین کنید.", statusCode: 401)
                        : ApiResponse<object>.Fail("احراز هویت ناموفق است.", statusCode: 401);

                    return context.Response.WriteAsJsonAsync(response);
                },

                OnChallenge = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    
                    var response = ApiResponse<object>.Fail("دسترسی ممنوع. لطفاً توکن معتبر ارسال کنید.", statusCode: 401);

                    return context.Response.WriteAsJsonAsync(response);
                }
            };
        });
    }
}