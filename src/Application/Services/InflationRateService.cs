using Application.Interfaces;
using Application.Models;
using Domain.Exceptions;

namespace Application.Services
{
    public class InflationRateService : IInflationRateService
    {
        private readonly IInflationService _inflationService;

        public InflationRateService(IInflationService inflationService)
        {
            _inflationService = inflationService;
        }

        public async Task<InflationRateResponse> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            InflationSeries? series = await _inflationService.GetSeriesAsync(cancellationToken);
            InflationRateResponse? latest = series?.Latest();

            if (latest is null)
            {
                throw new ExternalServiceException("No se pudo obtener la inflación desde datos.gob.ar.");
            }

            return latest;
        }
    }
}
