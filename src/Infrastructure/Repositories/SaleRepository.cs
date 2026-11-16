using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repositories
{
    // Hereda el contexto y el DbSet de Repository<Sale>; la venta se guarda junto con el stock en una transacción
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(ApplicationContext context) : base(context)
        {
        }

        public Sale Create(Sale newSale, List<BranchStock> updatedStocks)
        {
            newSale.Number = (_dbSet.Max(s => (int?)s.Number) ?? 0) + 1;

            _dbSet.Add(newSale);
            _context.BranchStocks.UpdateRange(updatedStocks);
            _context.SaveChanges(); // una sola transacción: venta + stock

            return newSale;
        }

        public List<Sale> GetByBranches(List<Guid> branchIds, DateTime from, DateTime to)
        {
            return _dbSet
                .Include(s => s.Details)
                .Where(s => branchIds.Contains(s.BranchId) && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }

        public List<Sale> GetByUser(string userCuit, DateTime from, DateTime to)
        {
            return _dbSet
                .Include(s => s.Details)
                .Where(s => s.UserCuit == userCuit && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }
    }
}
