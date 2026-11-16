using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using web.HealthChecks;

namespace web.Extensions;

public static class HealthCheckExtensions
{
    public const string HealthCheckPath = "/health";

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        return services;
    }

    public static IEndpointRouteBuilder MapApiHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(HealthCheckPath, new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter.WriteAsync })
            .AllowAnonymous();

        return endpoints;
    }
}
