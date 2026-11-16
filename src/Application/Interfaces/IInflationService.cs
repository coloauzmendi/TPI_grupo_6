using Application.Models;

namespace Application.Interfaces
{
    public interface IInflationService
    {
        // Serie del IPC nacional (INDEC); null si el servicio externo no está disponible
        Task<InflationSeries?> GetSeriesAsync(CancellationToken cancellationToken = default);
    }
}
