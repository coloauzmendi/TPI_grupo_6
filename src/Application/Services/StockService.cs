using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class StockService
    {
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly AccessService _accessService;

        public StockService(
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            AccessService accessService)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
        }

        // Stock activo de la sucursal con los datos de cada producto
        public List<StockItemResponse> GetItems(string cuit, Guid branchId)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

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

        public BranchStock GetById(string cuit, Guid branchId, Guid productId)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            return GetActive(branchId, productId);
        }

        // Da de alta un producto en la sucursal; si estaba dado de baja, se reactiva
        public BranchStock Create(string cuit, Guid branchId, BranchStock newStock)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);
            ValidateQuantities(newStock);

            if (_productRepository.GetById(newStock.ProductId) is null)
            {
                throw new AppValidationException("El producto indicado no existe.");
            }

            BranchStock? existing = _stockRepository.GetById(branchId, newStock.ProductId);
            if (existing is not null && existing.IsActive)
            {
                throw new ConflictException("Ese producto ya tiene stock cargado en esta sucursal.");
            }

            if (existing is not null)
            {
                existing.Quantity = newStock.Quantity;
                existing.MinimumStock = newStock.MinimumStock;
                existing.IsActive = true;
                _stockRepository.Update(existing);

                return existing;
            }

            newStock.BranchId = branchId;
            newStock.IsActive = true;

            return _stockRepository.Create(newStock);
        }

        public void Update(string cuit, Guid branchId, Guid productId, BranchStock updatedStock)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);
            ValidateQuantities(updatedStock);

            BranchStock stock = GetActive(branchId, productId);
            stock.Quantity = updatedStock.Quantity;
            stock.MinimumStock = updatedStock.MinimumStock;

            _stockRepository.Update(stock);
        }

        // Baja lógica del producto en la sucursal
        public void Delete(string cuit, Guid branchId, Guid productId)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            _stockRepository.Delete(GetActive(branchId, productId));
        }

        private BranchStock GetActive(Guid branchId, Guid productId)
        {
            BranchStock? stock = _stockRepository.GetById(branchId, productId);
            if (stock is null || !stock.IsActive)
            {
                throw new NotFoundException("El producto no está dado de alta en esta sucursal.");
            }

            return stock;
        }

        private static void ValidateQuantities(BranchStock stock)
        {
            if (stock.Quantity < 0 || stock.MinimumStock < 0)
            {
                throw new AppValidationException("La cantidad y el stock mínimo no pueden ser negativos.");
            }
        }
    }
}
