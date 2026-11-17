using Application.Models;
using Application.Services;
using Application.Tests.Fakes;
using Domain.Exceptions;

namespace Application.Tests
{
    public class InflationRateServiceTests
    {
        [Fact]
        public async Task GetLatestAsync_DevuelveLaTasaDelUltimoMesPublicado()
        {
            FakeInflationService inflation = new()
            {
                Series = new InflationSeries(new[]
                {
                    (new DateTime(2026, 8, 1), 100m),
                    (new DateTime(2026, 9, 1), 102m)
                })
            };

            InflationRateResponse latest = await new InflationRateService(inflation).GetLatestAsync();

            Assert.Equal(2026, latest.Year);
            Assert.Equal(9, latest.Month);
            Assert.Equal(2m, latest.Percent);
        }

        [Fact]
        public async Task GetLatestAsync_SinDatos_LanzaExternalServiceException()
        {
            FakeInflationService inflation = new() { Series = null };

            await Assert.ThrowsAsync<ExternalServiceException>(() => new InflationRateService(inflation).GetLatestAsync());
        }
    }
}
