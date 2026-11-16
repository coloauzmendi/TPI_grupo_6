namespace Application.Models
{
    // Inflación mensual de un mes puntual (Percent = 1.9 significa 1,9 %)
    public class InflationRateResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Percent { get; set; }
    }

    // Serie del índice de precios (IPC nacional) con la que se calculan tasas y se deflactan montos
    public class InflationSeries
    {
        private readonly SortedDictionary<DateTime, decimal> _index = new();

        public InflationSeries(IEnumerable<(DateTime Month, decimal Index)> points)
        {
            foreach (var (month, index) in points)
            {
                _index[new DateTime(month.Year, month.Month, 1)] = index;
            }
        }

        public decimal? IndexOf(int year, int month)
        {
            return _index.TryGetValue(new DateTime(year, month, 1), out decimal value) ? value : null;
        }

        // Variación porcentual contra el mes anterior; null si falta alguno de los dos datos
        public InflationRateResponse? RateOf(int year, int month)
        {
            DateTime previous = new DateTime(year, month, 1).AddMonths(-1);
            decimal? current = IndexOf(year, month);
            decimal? before = IndexOf(previous.Year, previous.Month);
            if (current is null || before is null || before == 0)
            {
                return null;
            }

            return new InflationRateResponse
            {
                Year = year,
                Month = month,
                Percent = Math.Round((current.Value / before.Value - 1) * 100, 2)
            };
        }

        // Último mes publicado
        public InflationRateResponse? Latest()
        {
            if (_index.Count == 0)
            {
                return null;
            }

            DateTime last = _index.Keys.Last();
            return RateOf(last.Year, last.Month);
        }
    }

    // Facturación del cierre mensual expresada en pesos del mes consultado
    public class RealIncomeResponse
    {
        // Inflación del mes consultado y acumulada de los últimos 12 meses
        public decimal MonthlyPercent { get; set; }
        public decimal AccumulatedPercent { get; set; }

        public decimal RealPrevious { get; set; }
        public decimal RealLastYear { get; set; }

        // Los mismos 12 meses de Months, deflactados a pesos del mes consultado
        public List<MonthTotalResponse> RealMonths { get; set; } = new();
    }
}
