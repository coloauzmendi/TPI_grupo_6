using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    // Datos del usuario logueado: arma las respuestas "me" a partir de su CUIT
    public class MeService
    {
        private readonly AccessService _accessService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISaleRepository _saleRepository;

        public MeService(
            AccessService accessService,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            ISaleRepository saleRepository)
        {
            _accessService = accessService;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _saleRepository = saleRepository;
        }

        public MeResponse GetMe(string cuit)
        {
            User user = _accessService.GetUser(cuit);

            return new MeResponse
            {
                Cuit = user.Cuit,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Branch = GetBranches(cuit).FirstOrDefault(b => b.Id == user.BranchId)
            };
        }

        public List<BranchResponse> GetBranches(string cuit)
        {
            Dictionary<Guid, string> companyNames = _accessService.GetCompanies(cuit).ToDictionary(c => c.Id, c => c.Name);

            return _accessService.GetBranches(cuit)
                .Select(b => new BranchResponse
                {
                    Id = b.Id,
                    Locality = b.Locality,
                    Address = b.Address,
                    Industry = b.Industry,
                    CompanyId = b.CompanyId,
                    CompanyName = companyNames.GetValueOrDefault(b.CompanyId, string.Empty)
                })
                .ToList();
        }

        // Empresas del dueño con sus dueños y sucursales (arma toda la pantalla de Empresas)
        public List<CompanyDetailResponse> GetCompanies(string cuit)
        {
            List<Branch> branches = _accessService.GetBranches(cuit);

            return _accessService.GetCompanies(cuit)
                .Select(c => new CompanyDetailResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Owners = _companyRepository.GetOwners(c.Id)
                        .Select(o => new OwnerSummary { Cuit = o.Cuit, FullName = $"{o.FirstName} {o.LastName}" })
                        .ToList(),
                    Branches = branches.Where(b => b.CompanyId == c.Id)
                        .Select(b => new BranchDetailResponse
                        {
                            Id = b.Id,
                            Locality = b.Locality,
                            Address = b.Address,
                            Industry = b.Industry,
                            EmployeeCount = _userRepository.GetByBranch(b.Id).Count
                        })
                        .ToList()
                })
                .ToList();
        }

        // Productos de las sucursales del usuario que están en el stock mínimo o por debajo
        public List<StockAlertResponse> GetStockAlerts(string cuit)
        {
            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);

            return _accessService.GetBranches(cuit)
                .SelectMany(b => _stockRepository.GetByBranch(b.Id))
                .Where(s => s.Quantity <= s.MinimumStock && products.ContainsKey(s.ProductId))
                .Select(s => new StockAlertResponse
                {
                    BranchId = s.BranchId,
                    ProductId = s.ProductId,
                    Name = products[s.ProductId].Name,
                    Brand = products[s.ProductId].Brand,
                    Quantity = s.Quantity,
                    MinimumStock = s.MinimumStock
                })
                .OrderBy(a => a.Quantity)
                .ToList();
        }

        public List<EmployeeResponse> GetEmployees(string cuit)
        {
            List<Guid> branchIds = _accessService.GetBranches(cuit).Select(b => b.Id).ToList();

            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            Dictionary<string, int> salesByUser = _saleRepository
                .GetByBranches(branchIds, monthStart, monthStart.AddMonths(1))
                .GroupBy(s => s.UserCuit)
                .ToDictionary(g => g.Key, g => g.Count());

            return branchIds
                .SelectMany(id => _userRepository.GetByBranch(id))
                .Where(u => u.Role == UserRole.Employee)
                .Select(u => new EmployeeResponse
                {
                    Cuit = u.Cuit,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    BranchId = u.BranchId!.Value,
                    MonthSales = salesByUser.GetValueOrDefault(u.Cuit)
                })
                .OrderBy(e => e.LastName)
                .ToList();
        }
    }
}