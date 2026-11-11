using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AegisFlow.Api;

/// <summary>
/// Configures signed JWT validation and read/write policies. Production should
/// use asymmetric tokens from a trusted OIDC issuer rather than local HS256.
/// </summary>
public static class ApiAuthentication
{
    public static IServiceCollection AddOrderApiAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Auth:SigningKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException(
                "Auth:SigningKey must be configured with at least 32 random bytes.");
        var issuer = configuration["Auth:Issuer"] ?? "aegisflow-local";
        var audience = configuration["Auth:Audience"] ?? "aegisflow-api";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(10),
                    NameClaimType = "sub"
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy("inventory.read", p => p.RequireAuthenticatedUser()
                .RequireClaim("scope", "inventory.read", "inventory.write"))
            .AddPolicy("inventory.write", p => p.RequireAuthenticatedUser()
                .RequireClaim("scope", "inventory.write"))
            .AddPolicy("orders.read", p => p.RequireAuthenticatedUser()
                .RequireClaim("scope", "orders.read", "orders.write"))
            .AddPolicy("orders.write", p => p.RequireAuthenticatedUser()
                .RequireClaim("scope", "orders.write"));
        return services;
    }
}
