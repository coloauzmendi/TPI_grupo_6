using Application.Models;
using Domain.Entities;

namespace Application.Services
{
    // Arma las respuestas de venta y compra con el nombre de cada producto
    public static class SaleMapper
    {
        public static SaleResponse ToResponse(Sale sale, Dictionary<Guid, Product> products)
        {
            return new SaleResponse
            {
                Id = sale.Id,
                Code = $"V-{sale.Number:D6}",
                BranchId = sale.BranchId,
                Date = sale.Date,
                Total = sale.Total,
                Items = sale.Details.Select(d => new SaleItemResponse
                {
                    ProductId = d.ProductId,
                    ProductName = products.TryGetValue(d.ProductId, out Product? p) ? p.Name : string.Empty,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Subtotal = d.Quantity * d.UnitPrice
                }).ToList()
            };
        }
    }
}