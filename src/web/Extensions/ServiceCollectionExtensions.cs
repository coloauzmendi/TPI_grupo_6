using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IBranchStockRepository, BranchStockRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IMeService, MeService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInflationRateService, InflationRateService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();

        // Servicio de terceros: inflación (IPC del INDEC) desde datos.gob.ar, con HttpClient creado por IHttpClientFactory
        services.AddMemoryCache();
        services.AddHttpClient<IInflationService, InflationService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:SeriesBaseUrl"] ?? "https://apis.datos.gob.ar/series/api/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
