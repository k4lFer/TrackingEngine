using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Config;

public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Security configurations can be added here in the future.
        #region JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(jwtBearerOptions =>
        {
            jwtBearerOptions.SaveToken = true;
            jwtBearerOptions.RequireHttpsMetadata = !environment.IsDevelopment();
            jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:AccessTokenKey"])),
                ClockSkew = TimeSpan.Zero,
            };
            // Eventos
            jwtBearerOptions.Events = new JwtBearerEvents
            {
                /*OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },*/
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    else if (context.Exception is SecurityTokenInvalidSignatureException)
                    {
                        context.Response.Headers["Token-Invalid"] = "true";
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{ \"status\": 401, \"error\": \"Unauthorized\", \"message\": \"Token no valido o expirado.\"}");

                }
            };
        });
        services.AddAuthorization();
        #endregion
        return services;
    }

}