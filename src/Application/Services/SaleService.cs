using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAccessService _accessService;

        public SaleService(
            ISaleRepository saleRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            IAccessService accessService)
        {
            _saleRepository = saleRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
        }

        public SaleResultResponse Create(string cuit, Guid branchId, SaleRequest request)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);
            ValidateLines(request.Items);

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);
            Sale sale = new Sale
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                UserCuit = cuit,
                Date = DateTime.Now
            };
            List<BranchStock> updatedStocks = new();
            List<string> alerts = new();

            // Si el mismo producto viene repetido se suman las cantidades
            foreach (var group in request.Items.GroupBy(i => i.ProductId))
            {
                long quantity = group.Sum(i => (long)i.Quantity);

                BranchStock? stock = _stockRepository.GetById(branchId, group.Key);
                if (stock is null || !stock.IsActive || !products.TryGetValue(group.Key, out Product? product))
                {
                    throw new AppValidationException("El producto no está disponible en esta sucursal.");
                }

                if (quantity > stock.Quantity)
                {
                    throw new AppValidationException($"Stock insuficiente para {product.Name}.");
                }

                stock.Quantity -= (int)quantity;
                updatedStocks.Add(stock);

                // El precio queda guardado: si el producto cambia de precio, esta venta no se modifica
                sale.Details.Add(new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = (int)quantity,
                    UnitPrice = product.Price
                });

                if (stock.Quantity <= stock.MinimumStock)
                {
                    alerts.Add($"{product.Name} quedó en {stock.Quantity} u. (mínimo {stock.MinimumStock})");
                }
            }

            sale.Total = sale.Details.Sum(d => d.Quantity * d.UnitPrice);
            _saleRepository.Create(sale, updatedStocks);

            return new SaleResultResponse
            {
                Sale = SaleMapper.ToResponse(sale, products),
                Alerts = alerts
            };
        }

        private static void ValidateLines(List<SaleItemRequest> items)
        {
            if (items.Any(i => i.Quantity <= 0))
            {
                throw new AppValidationException("La cantidad de cada producto debe ser mayor a 0.");
            }
        }
    }
}