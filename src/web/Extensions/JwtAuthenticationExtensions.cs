using System.Text;
using Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;

namespace web.Extensions;

public static class JwtAuthenticationExtensions
{
    // Autenticación con JWT: qué se verifica del token que viene en cada request y contra qué valores
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        string jwtIssuer = configuration.GetRequiredSetting("AuthenticationService:Issuer");
        string jwtAudience = configuration.GetRequiredSetting("AuthenticationService:Audience");
        string jwtSecret = configuration.GetRequiredSetting("AuthenticationService:SecretForKey");

        // HMAC-SHA256 exige una clave de al menos 256 bits; si no, el error aparecería recién al primer login
        if (Encoding.ASCII.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException("AuthenticationService:SecretForKey debe tener al menos 32 caracteres.");
        }

        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                // Se conservan los nombres de los claims del token ("sub", "role") en vez de mapearlos a los de Microsoft
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new()
                {
                    NameClaimType = "sub",
                    RoleClaimType = "role", // permite usar [Authorize(Roles = "Owner")]
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true, // la fecha de expiración se valida por defecto
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret))
                };
            });

        services.Configure<AuthenticationServiceOptions>(
            configuration.GetSection(AuthenticationServiceOptions.AuthenticationService));

        return services;
    }
}
