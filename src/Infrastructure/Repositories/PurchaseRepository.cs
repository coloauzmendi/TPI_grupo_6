using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    // Hereda el contexto y el DbSet de Repository<Purchase>; la compra se guarda junto con el stock en una transacción
    public class PurchaseRepository : Repository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(ApplicationContext context) : base(context)
        {
        }

        public Purchase Create(Purchase newPurchase, List<BranchStock> updatedStocks)
        {
            newPurchase.Number = (_dbSet.Max(p => (int?)p.Number) ?? 0) + 1;

            _dbSet.Add(newPurchase);
            _context.BranchStocks.UpdateRange(updatedStocks);
            _context.SaveChanges(); // una sola transacción: compra + stock

            return newPurchase;
        }
    }
}
