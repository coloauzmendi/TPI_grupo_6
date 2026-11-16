using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class PriceService
    {
        private const decimal MinPercent = -99m;
        private const decimal MaxPercent = 1000m;

        private readonly IProductRepository _productRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly AccessService _accessService;

        public PriceService(
            IProductRepository productRepository,
            IBranchStockRepository stockRepository,
            AccessService accessService)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _accessService = accessService;
        }

        // Devuelve la cantidad de productos actualizados
        public int Update(string cuit, PriceUpdateRequest request)
        {
            ValidateValue(request);

            List<Guid> companyIds = ResolveCompanies(cuit, request.CompanyId);

            // Solo se tocan los productos de esas empresas que tienen stock activo en sus sucursales
            HashSet<Guid> productIds = _accessService.GetBranches(cuit)
                .Where(b => companyIds.Contains(b.CompanyId))
                .SelectMany(b => _stockRepository.GetByBranch(b.Id))
                .Select(s => s.ProductId)
                .ToHashSet();

            List<Product> scope = _productRepository.GetByCompanies(companyIds).Where(p => productIds.Contains(p.Id)).ToList();
            List<Product> targets = request.Mode switch
            {
                PriceMode.Individual => scope.Where(p => p.Id.ToString() == request.Target).ToList(),
                PriceMode.Category => scope.Where(p => p.Category == request.Target).ToList(),
                PriceMode.Industry => scope.Where(p => p.Industry.ToString() == request.Target).ToList(),
                _ => scope
            };

            if (targets.Count == 0)
            {
                throw new AppValidationException("No hay productos alcanzados por la actualización.");
            }

            foreach (Product product in targets)
            {
                decimal newPrice = request.Mode == PriceMode.Individual
                    ? request.Value
                    : Math.Round(product.Price * (1 + request.Value / 100m), 2);

                if (newPrice <= 0)
                {
                    throw new AppValidationException("Ningún precio puede quedar en 0 o menos.");
                }

                if (newPrice > ValidationLimits.MaxAmount)
                {
                    throw new AppValidationException("Ningún precio puede superar 99.999.999,99.");
                }

                product.Price = newPrice;
            }

            _productRepository.UpdateRange(targets);

            return targets.Count;
        }

        // Todas las empresas del dueño, o solo la indicada si es suya
        private List<Guid> ResolveCompanies(string cuit, Guid? companyId)
        {
            if (companyId is not null)
            {
                return new List<Guid> { _accessService.EnsureCompanyOwner(cuit, companyId.Value).Id };
            }

            return _accessService.GetCompanies(cuit).Select(c => c.Id).ToList();
        }

        private static void ValidateValue(PriceUpdateRequest request)
        {
            if (request.Mode == PriceMode.Individual)
            {
                if (request.Value < ValidationLimits.MinAmount || request.Value > ValidationLimits.MaxAmount)
                {
                    throw new AppValidationException("El precio nuevo debe estar entre 0,01 y 99.999.999,99.");
                }

                return;
            }

            if (request.Value < MinPercent || request.Value > MaxPercent)
            {
                throw new AppValidationException("El porcentaje debe estar entre -99 y 1000.");
            }
        }
    }
}