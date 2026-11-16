using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAccessService _accessService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            IAccessService accessService)
        {
            _purchaseRepository = purchaseRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
        }

        public PurchaseResponse Create(string cuit, Guid branchId, PurchaseRequest request)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);
            ValidateLines(request.Items);
            Dictionary<PurchaseItemRequest, decimal> unitCosts = request.Items
                .ToDictionary(i => i, i => UnitCostCalculator.Resolve(i.UnitCost, i.TotalCost, i.Quantity));

            Dictionary<Guid, Product> products = _productRepository.GetAll().ToDictionary(p => p.Id);
            Purchase purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                UserCuit = cuit,
                Date = DateTime.Now,
                Supplier = string.IsNullOrWhiteSpace(request.Supplier) ? null : request.Supplier.Trim()
            };
            List<BranchStock> updatedStocks = new();
            Dictionary<Guid, int> newStocks = new();

            // Si el mismo producto viene repetido con el mismo costo se suman las cantidades
            foreach (var group in request.Items.GroupBy(i => i.ProductId))
            {
                decimal unitCost = unitCosts[group.First()];
                long quantity = group.Sum(i => (long)i.Quantity);

                BranchStock? stock = _stockRepository.GetById(branchId, group.Key);
                if (stock is null || !stock.IsActive || !products.TryGetValue(group.Key, out Product? product))
                {
                    throw new AppValidationException("El producto no está dado de alta en esta sucursal.");
                }

                if (group.Any(i => unitCosts[i] != unitCost))
                {
                    throw new AppValidationException($"{product.Name} viene repetido con costos distintos.");
                }

                StockService.EnsureMaxStock(stock.Quantity + quantity);

                stock.Add((int)quantity);
                updatedStocks.Add(stock);
                newStocks[group.Key] = stock.Quantity;

                purchase.Details.Add(new PurchaseDetail
                {
                    PurchaseId = purchase.Id,
                    ProductId = group.Key,
                    Quantity = (int)quantity,
                    UnitCost = unitCost
                });
            }

            purchase.Total = purchase.Details.Sum(d => d.Quantity * d.UnitCost);
            _purchaseRepository.Create(purchase, updatedStocks);

            return new PurchaseResponse
            {
                Id = purchase.Id,
                Code = $"C-{purchase.Number:D5}",
                BranchId = purchase.BranchId,
                Date = purchase.Date,
                Supplier = purchase.Supplier,
                Total = purchase.Total,
                Items = purchase.Details.Select(d => new PurchaseItemResponse
                {
                    ProductId = d.ProductId,
                    ProductName = products[d.ProductId].Name,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost,
                    NewStock = newStocks[d.ProductId]
                }).ToList()
            };
        }

        private static void ValidateLines(List<PurchaseItemRequest> items)
        {
            if (items.Any(i => i.Quantity <= 0))
            {
                throw new AppValidationException("La cantidad de cada producto debe ser mayor a 0.");
            }
        }
    }
}