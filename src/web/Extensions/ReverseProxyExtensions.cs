using Microsoft.AspNetCore.HttpOverrides;

namespace web.Extensions;

public static class ReverseProxyExtensions
{
    public static IServiceCollection AddReverseProxySupport(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IApplicationBuilder UseReverseProxySupport(this IApplicationBuilder app)
    {
        return app.UseForwardedHeaders();
    }

    public static IApplicationBuilder UseProxyAwareHttpsRedirection(this IApplicationBuilder app)
    {
        return app.UseWhen(
            context => !context.Request.Path.StartsWithSegments(HealthCheckExtensions.HealthCheckPath),
            branch => branch.UseHttpsRedirection());
    }
}
