using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
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
            newPurchase.Number = (_context.Purchases.Max(p => (int?)p.Number) ?? 0) + 1;

            _context.Purchases.Add(newPurchase);
            _context.BranchStocks.UpdateRange(updatedStocks);
            _context.SaveChanges(); // una sola transacción: compra + stock

            return newPurchase;
        }
    }
}