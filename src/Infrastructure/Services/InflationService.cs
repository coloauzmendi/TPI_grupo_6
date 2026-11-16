using System.Globalization;
using System.Text.Json;
using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    // Consume la API de Series de Tiempo de datos.gob.ar (IPC nacional, INDEC); no requiere cuenta ni clave
    public class InflationService : IInflationService
    {
        private const string CacheKey = "inflation-series";
        // Es una serie mensual: alcanza con consultarla un par de veces por día
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

        // IPC Nivel General Nacional, base dic-2016, mensual
        private const string SeriesPath = "series/?ids=148.3_INIVELNAL_DICI_M_26&limit=60&sort=desc&format=json";

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<InflationService> _logger;

        public InflationService(HttpClient httpClient, IMemoryCache cache, ILogger<InflationService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<InflationSeries?> GetSeriesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out InflationSeries? cached))
            {
                return cached;
            }

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(SeriesPath, cancellationToken);
                response.EnsureSuccessStatusCode();

                using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

                // Cada fila es ["2026-09-01", 123.45]
                List<(DateTime Month, decimal Index)> points = new();
                foreach (JsonElement row in document.RootElement.GetProperty("data").EnumerateArray())
                {
                    if (row[1].ValueKind != JsonValueKind.Number)
                    {
                        continue;
                    }

                    points.Add((DateTime.ParseExact(row[0].GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture), row[1].GetDecimal()));
                }

                InflationSeries series = new(points);
                _cache.Set(CacheKey, series, CacheDuration);
                return series;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
            {
                // Un fallo del servicio de terceros no tiene que romper la API: quien llama decide qué hacer sin el dato
                _logger.LogWarning(ex, "No se pudo obtener la serie de inflación de datos.gob.ar");
                return null;
            }
        }
    }
}
