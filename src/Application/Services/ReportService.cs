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

        public ReportService(
            ISaleRepository saleRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            AccessService accessService)
        {
            _saleRepository = saleRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
        }

        // Ventas de hoy: el dueño ve las de todas sus sucursales; el empleado, solo las que registró él
        public TodayResponse GetToday(string cuit)
        {
            User user = _accessService.GetUser(cuit);
            DateTime today = DateTime.Today;

            List<Sale> sales = user.Role == UserRole.Owner
                ? _saleRepository.GetByBranches(_accessService.GetBranches(cuit).Select(b => b.Id).ToList(), today, today.AddDays(1))
                : _saleRepository.GetByUser(cuit, today, today.AddDays(1));

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);

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

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);
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

        public MonthlyCloseResponse GetMonthlyClose(string cuit, Guid? companyId, Guid? branchId, int year, int month)
        {
            ValidatePeriod(year, month);
            List<Guid> branchIds = ResolveBranches(cuit, companyId, branchId);

            DateTime current = new DateTime(year, month, 1);
            // Se leen de una vez los 12 meses anteriores (incluye el mismo mes del año pasado)
            List<Sale> sales = _saleRepository.GetByBranches(branchIds, current.AddMonths(-12), current.AddMonths(1));

            List<Sale> InMonth(DateTime m) => sales.Where(s => s.Date >= m && s.Date < m.AddMonths(1)).ToList();
            List<Sale> currentSales = InMonth(current);

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);

            return new MonthlyCloseResponse
            {
                Current = ToKpi(currentSales),
                Previous = ToKpi(InMonth(current.AddMonths(-1))),
                LastYear = ToKpi(InMonth(current.AddMonths(-12))),
                Months = Enumerable.Range(-11, 12)
                    .Select(i => current.AddMonths(i))
                    .Select(m => new MonthTotalResponse { Year = m.Year, Month = m.Month, Total = InMonth(m).Sum(s => s.Total) })
                    .ToList(),
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