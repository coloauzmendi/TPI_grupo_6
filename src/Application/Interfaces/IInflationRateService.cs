using Application.Models;

namespace Application.Interfaces
{
    public interface IInflationRateService
    {
        // Inflación mensual del último mes publicado
        Task<InflationRateResponse> GetLatestAsync(CancellationToken cancellationToken = default);
    }
}
