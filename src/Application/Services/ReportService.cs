using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class ReportService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly AccessService _accessService;
        private readonly IInflationService _inflationService;

        public ReportService(
            ISaleRepository saleRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            AccessService accessService,
            IInflationService inflationService)
        {
            _saleRepository = saleRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
            _inflationService = inflationService;
        }

        // Ventas de hoy: el dueño ve las de todas sus sucursales; el empleado, solo las que registró él
        public TodayResponse GetToday(string cuit)
        {
            User user = _accessService.GetUser(cuit);
            DateTime today = DateTime.Today;

            List<Sale> sales = user.Role == UserRole.Owner
                ? _saleRepository.GetByBranches(_accessService.GetBranches(cuit).Select(b => b.Id).ToList(), today, today.AddDays(1))
                : _saleRepository.GetByUser(cuit, today, today.AddDays(1));

            Dictionary<Guid, Product> products = _productRepository.GetAllIncludingInactive().ToDictionary(p => p.Id);

            return new TodayResponse
            {
                Kpi = ToKpi(sales),
                Recent = sales.OrderByDescending(s => s.Date).Take(5).Select(s => SaleMapper.ToResponse(s, products)).ToList()
            };
        }

        public RankingResponse GetRanking(string cuit, Guid? companyId, Guid? branchId, int year, int month, bool mostSold)
        {
            ValidatePeriod(year, month);
            List<Guid> branchIds = ResolveBranches(cuit, companyId, branchId);

            DateTime from = new DateTime(year, month, 1);
            List<Sale> sales = _saleRepository.GetByBranches(branchIds, from, from.AddMonths(1));

            Dictionary<Guid, Product> products = _productRepository.GetAllIncludingInactive().ToDictionary(p => p.Id);
            Dictionary<Guid, (int Units, decimal Amount)> sold = sales
                .SelectMany(s => s.Details)
                .GroupBy(d => d.ProductId)
                .ToDictionary(g => g.Key, g => (g.Sum(d => d.Quantity), g.Sum(d => d.Quantity * d.UnitPrice)));

            // Entran al ranking los productos con stock activo en el alcance, aunque no hayan vendido
            HashSet<Guid> productIds = branchIds
                .SelectMany(id => _stockRepository.GetByBranch(id))
                .Select(s => s.ProductId)
                .Concat(sold.Keys)
                .ToHashSet();

            IEnumerable<RankingRowResponse> rows = productIds
                .Where(products.ContainsKey)
                .Select(id => new RankingRowResponse
                {
                    ProductId = id,
                    Name = products[id].Name,
                    Category = products[id].Category,
                    Industry = products[id].Industry,
                    Units = sold.GetValueOrDefault(id).Units,
                    Amount = sold.GetValueOrDefault(id).Amount
                });

            rows = mostSold
                ? rows.OrderByDescending(r => r.Units).ThenBy(r => r.Name)
                : rows.OrderBy(r => r.Units).ThenBy(r => r.Name);

            return new RankingResponse
            {
                Kpi = ToKpi(sales),
                Rows = rows.Take(15).ToList()
            };
        }

        public async Task<MonthlyCloseResponse> GetMonthlyCloseAsync(string cuit, Guid? companyId, Guid? branchId, int year, int month)
        {
            ValidatePeriod(year, month);
            List<Guid> branchIds = ResolveBranches(cuit, companyId, branchId);

            DateTime current = new DateTime(year, month, 1);
            // Se leen de una vez los 12 meses anteriores (incluye el mismo mes del año pasado)
            List<Sale> sales = _saleRepository.GetByBranches(branchIds, current.AddMonths(-12), current.AddMonths(1));

            List<Sale> InMonth(DateTime m) => sales.Where(s => s.Date >= m && s.Date < m.AddMonths(1)).ToList();
            List<Sale> currentSales = InMonth(current);

            Dictionary<Guid, Product> products = _productRepository.GetAllIncludingInactive().ToDictionary(p => p.Id);

            List<MonthTotalResponse> months = Enumerable.Range(-11, 12)
                .Select(i => current.AddMonths(i))
                .Select(m => new MonthTotalResponse { Year = m.Year, Month = m.Month, Total = InMonth(m).Sum(s => s.Total) })
                .ToList();
            KpiResponse previousKpi = ToKpi(InMonth(current.AddMonths(-1)));
            KpiResponse lastYearKpi = ToKpi(InMonth(current.AddMonths(-12)));

            return new MonthlyCloseResponse
            {
                Current = ToKpi(currentSales),
                Previous = previousKpi,
                LastYear = lastYearKpi,
                Months = months,
                RealIncome = await GetRealIncomeAsync(current, months, previousKpi.Total, lastYearKpi.Total),
                Top = currentSales
                    .SelectMany(s => s.Details)
                    .GroupBy(d => d.ProductId)
                    .Select(g => new TopProductResponse
                    {
                        Name = products.TryGetValue(g.Key, out Product? p) ? p.Name : string.Empty,
                        Units = g.Sum(d => d.Quantity)
                    })
                    .OrderByDescending(t => t.Units)
                    .Take(5)
                    .ToList(),
                Breakdown = sales
                    .Where(s => s.Date >= current.AddMonths(-11))
                    .SelectMany(s => s.Details)
                    .GroupBy(d => d.ProductId)
                    .Where(g => products.ContainsKey(g.Key))
                    .Select(g => new BreakdownRowResponse
                    {
                        ProductId = g.Key,
                        Name = products[g.Key].Name,
                        Category = products[g.Key].Category,
                        Industry = products[g.Key].Industry,
                        Units = g.Sum(d => d.Quantity),
                        Amount = g.Sum(d => d.Quantity * d.UnitPrice)
                    })
                    .OrderByDescending(r => r.Amount)
                    .ToList(),
                Branches = branchIds.Select(id =>
                {
                    KpiResponse kpi = ToKpi(currentSales.Where(s => s.BranchId == id).ToList());
                    return new BranchClosingResponse { BranchId = id, Sales = kpi.Sales, Units = kpi.Units, Total = kpi.Total };
                }).ToList()
            };
        }

        // Expresa la facturación en pesos del mes consultado usando el IPC; null si falta el dato de algún mes
        private async Task<RealIncomeResponse?> GetRealIncomeAsync(DateTime current, List<MonthTotalResponse> months, decimal previousTotal, decimal lastYearTotal)
        {
            InflationSeries? series = await _inflationService.GetSeriesAsync();
            InflationRateResponse? rate = series?.RateOf(current.Year, current.Month);
            if (series is null || rate is null)
            {
                return null;
            }

            // Cuántos pesos del mes consultado equivalen a 1 peso de ese otro mes
            decimal? Factor(DateTime m)
            {
                decimal? now = series.IndexOf(current.Year, current.Month);
                decimal? then = series.IndexOf(m.Year, m.Month);
                return now is null || then is null || then == 0 ? null : now / then;
            }

            decimal? yearFactor = Factor(current.AddYears(-1));
            decimal? monthFactor = Factor(current.AddMonths(-1));
            if (yearFactor is null || monthFactor is null)
            {
                return null;
            }

            List<MonthTotalResponse> realMonths = new();
            foreach (MonthTotalResponse m in months)
            {
                decimal? factor = Factor(new DateTime(m.Year, m.Month, 1));
                if (factor is null)
                {
                    return null;
                }

                realMonths.Add(new MonthTotalResponse { Year = m.Year, Month = m.Month, Total = Math.Round(m.Total * factor.Value, 2) });
            }

            return new RealIncomeResponse
            {
                MonthlyPercent = rate.Percent,
                AccumulatedPercent = Math.Round((yearFactor.Value - 1) * 100, 2),
                RealPrevious = Math.Round(previousTotal * monthFactor.Value, 2),
                RealLastYear = Math.Round(lastYearTotal * yearFactor.Value, 2),
                RealMonths = realMonths
            };
        }

        // Alcance del reporte: una sucursal, una empresa o todas las del dueño (siempre dentro de lo que le pertenece)
        private List<Guid> ResolveBranches(string cuit, Guid? companyId, Guid? branchId)
        {
            if (companyId is not null)
            {
                _accessService.EnsureCompanyOwner(cuit, companyId.Value);
            }

            if (branchId is not null)
            {
                return new List<Guid> { _accessService.EnsureBranchAccess(cuit, branchId.Value).Id };
            }

            return _accessService.GetBranches(cuit)
                .Where(b => companyId is null || b.CompanyId == companyId)
                .Select(b => b.Id)
                .ToList();
        }

        private static void ValidatePeriod(int year, int month)
        {
            if (month < 1 || month > 12 || year < 2000 || year > 2100)
            {
                throw new AppValidationException("El período indicado no es válido.");
            }
        }

        private static KpiResponse ToKpi(List<Sale> sales)
        {
            return new KpiResponse
            {
                Sales = sales.Count,
                Units = sales.Sum(s => s.Details.Sum(d => d.Quantity)),
                Total = sales.Sum(s => s.Total)
            };
        }
    }
}