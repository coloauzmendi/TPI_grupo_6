using Application.Interfaces;
using Application.Models;
using Domain.Entities;

namespace Application.Services
{
    public class StockService
    {
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public StockService(IBranchStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        // Stock activo de la sucursal con los datos de cada producto
        public List<StockItemResponse> GetItems(Guid branchId)
        {
            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);

            return _stockRepository.GetByBranch(branchId)
                .Where(s => products.ContainsKey(s.ProductId))
                .Select(s =>
                {
                    Product p = products[s.ProductId];
                    return new StockItemResponse
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        Brand = p.Brand,
                        Category = p.Category,
                        Industry = p.Industry,
                        Price = p.Price,
                        Quantity = s.Quantity,
                        MinimumStock = s.MinimumStock
                    };
                })
                .OrderBy(i => i.Name)
                .ToList();
        }
    }
}