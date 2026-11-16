namespace web.Extensions;

public static class CorsExtensions
{
    private const string FrontendCorsPolicy = "Frontend";

    // El FrontEnd (Vite) corre en otro origen, así que hay que permitirlo.
    // Cors:AllowedOrigins puede ser un arreglo en appsettings o, como variable de entorno,
    // una lista separada por comas (Cors__AllowedOrigins=https://a.com,https://b.com)
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection corsSection = configuration.GetSection("Cors:AllowedOrigins");
        string[] allowedOrigins = corsSection.GetChildren().Any()
            ? corsSection.Get<string[]>()!
            : (corsSection.Value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (allowedOrigins.Length == 0)
        {
            allowedOrigins = ["http://localhost:5173"];
        }

        services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }

    public static IApplicationBuilder UseFrontendCors(this IApplicationBuilder app)
    {
        return app.UseCors(FrontendCorsPolicy);
    }
}
