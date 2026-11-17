using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace web.HealthChecks;

public static class HealthCheckResponseWriter
{
    private const string VersionKey = "App:Version";

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        IConfiguration configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var response = new
        {
            status = report.Status.ToString(),
            version = configuration[VersionKey] ?? string.Empty
        };

        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
