using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class StockService
    {
        // Tope del stock de un producto en una sucursal
        public const int MaxStock = 1_000_000;

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

        // Stock activo de la sucursal con los datos de cada producto; el dueño puede pedir también el dado de baja
        public List<StockItemResponse> GetItems(string cuit, Guid branchId, bool includeInactive = false)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);
            List<BranchStock> stocks = includeInactive && _accessService.GetUser(cuit).Role == UserRole.Owner
                ? _stockRepository.GetByBranchIncludingInactive(branchId)
                : _stockRepository.GetByBranch(branchId);

            return stocks
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
                        MinimumStock = s.MinimumStock,
                        IsActive = s.IsActive
                    };
                })
                .OrderBy(i => i.Name)
                .ToList();
        }

        public BranchStockResponse GetById(string cuit, Guid branchId, Guid productId)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            return ResponseMapper.ToResponse(GetActive(branchId, productId));
        }

        // Si el producto estaba dado de baja en la sucursal, se reactiva
        public BranchStockResponse Create(string cuit, Guid branchId, BranchStockCreateRequest request)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            if (_productRepository.GetById(request.ProductId) is null)
            {
                throw new AppValidationException("El producto indicado no existe.");
            }

            BranchStock? existing = _stockRepository.GetById(branchId, request.ProductId);
            if (existing is not null && existing.IsActive)
            {
                throw new ConflictException("Ese producto ya tiene stock cargado en esta sucursal.");
            }

            EnsureMaxStock(request.Quantity);

            if (existing is null)
            {
                BranchStock created = _stockRepository.Create(new BranchStock
                {
                    BranchId = branchId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    MinimumStock = request.MinimumStock
                });

                return ResponseMapper.ToResponse(created);
            }

            existing.Quantity = request.Quantity;
            existing.MinimumStock = request.MinimumStock;
            existing.IsActive = true;
            _stockRepository.Update(existing);

            return ResponseMapper.ToResponse(existing);
        }

        public void Update(string cuit, Guid branchId, Guid productId, BranchStockUpdateRequest request)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);
            BranchStock stock = GetActive(branchId, productId);

            stock.Quantity = request.Quantity;
            stock.MinimumStock = request.MinimumStock;
            _stockRepository.Update(stock);
        }

        public static void EnsureMaxStock(long quantity)
        {
            if (quantity > MaxStock)
            {
                throw new AppValidationException("El stock resultante no puede superar las 1.000.000 unidades.");
            }
        }

        // Baja lógica
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
                throw new NotFoundException("El producto no tiene stock en esta sucursal.");
            }

            return stock;
        }
    }
}
