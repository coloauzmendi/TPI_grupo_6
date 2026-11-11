using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class PurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly AccessService _accessService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IBranchStockRepository stockRepository,
            IProductRepository productRepository,
            AccessService accessService)
        {
            _purchaseRepository = purchaseRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _accessService = accessService;
        }

        public PurchaseResponse Create(string cuit, Guid branchId, PurchaseRequest request)
        {
            _accessService.EnsureBranchAccess(cuit, branchId);

            if (request.Items.Count == 0)
            {
                throw new AppValidationException("La compra debe tener al menos un producto.");
            }

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

            foreach (var group in request.Items.GroupBy(i => i.ProductId))
            {
                PurchaseItemRequest first = group.First();
                int quantity = group.Sum(i => i.Quantity);
                if (quantity <= 0)
                {
                    throw new AppValidationException("La cantidad debe ser mayor a 0.");
                }

                if (first.UnitCost < 0)
                {
                    throw new AppValidationException("El costo unitario no puede ser negativo.");
                }

                BranchStock? stock = _stockRepository.GetById(branchId, group.Key);
                if (stock is null || !stock.IsActive || !products.ContainsKey(group.Key))
                {
                    throw new AppValidationException("El producto no está dado de alta en esta sucursal.");
                }

                stock.Quantity += quantity;
                updatedStocks.Add(stock);
                newStocks[group.Key] = stock.Quantity;

                purchase.Details.Add(new PurchaseDetail
                {
                    PurchaseId = purchase.Id,
                    ProductId = group.Key,
                    Quantity = quantity,
                    UnitCost = first.UnitCost
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
    }
}