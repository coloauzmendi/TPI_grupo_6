using Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace web.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationContext _context;

    public DatabaseHealthCheck(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
    }
}
