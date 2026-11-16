using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly ApplicationContext _context;

        public PurchaseRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Purchase Create(Purchase newPurchase, List<BranchStock> updatedStocks)
        {
            _context.Purchases.Add(newPurchase);

            // El stock que se da de alta en la sucursal todavía no existe: se inserta en lugar de actualizarse
            foreach (BranchStock stock in updatedStocks)
            {
                bool exists = _context.Entry(stock).State != EntityState.Detached
                    || _context.BranchStocks.Any(s => s.BranchId == stock.BranchId && s.ProductId == stock.ProductId);

                if (exists)
                {
                    _context.BranchStocks.Update(stock);
                }
                else
                {
                    _context.BranchStocks.Add(stock);
                }
            }

            // una sola transacción: compra + stock
            NumberConflictRetry.SaveChanges(_context, nameof(ApplicationContext.Purchases),
                () => newPurchase.Number = (_context.Purchases.Max(p => (int?)p.Number) ?? 0) + 1);

            return newPurchase;
        }

        public Dictionary<Guid, decimal> GetLatestUnitCosts(List<Guid> branchIds)
        {
            var details = _context.Purchases
                .Where(p => branchIds.Contains(p.BranchId))
                .SelectMany(p => p.Details, (p, d) => new { d.ProductId, p.Date, p.Number, d.UnitCost })
                .ToList();

            return details
                .GroupBy(d => d.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(d => d.Date).ThenByDescending(d => d.Number).First().UnitCost);
        }
    }
}