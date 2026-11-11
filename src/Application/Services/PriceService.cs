using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class PriceService
    {
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
            // Solo se tocan los productos que tienen stock activo en las sucursales del dueño
            HashSet<Guid> productIds = _accessService.GetBranches(cuit)
                .SelectMany(b => _stockRepository.GetByBranch(b.Id))
                .Select(s => s.ProductId)
                .ToHashSet();

            List<Product> scope = _productRepository.GetAll().Where(p => productIds.Contains(p.Id)).ToList();
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

                if (newPrice < 0)
                {
                    throw new AppValidationException("Ningún precio puede quedar menor a 0.");
                }

                product.Price = newPrice;
            }

            _productRepository.UpdateRange(targets);

            return targets.Count;
        }
    }
}