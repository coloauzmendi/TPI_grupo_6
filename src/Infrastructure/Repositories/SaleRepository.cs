using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly ApplicationContext _context;

        public SaleRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Sale Create(Sale newSale, List<BranchStock> updatedStocks)
        {
            newSale.Number = (_context.Sales.Max(s => (int?)s.Number) ?? 0) + 1;

            _context.Sales.Add(newSale);
            _context.BranchStocks.UpdateRange(updatedStocks);
            _context.SaveChanges(); // una sola transacción: venta + stock

            return newSale;
        }

        public List<Sale> GetByBranches(List<Guid> branchIds, DateTime from, DateTime to)
        {
            return _context.Sales
                .Include(s => s.Details)
                .Where(s => branchIds.Contains(s.BranchId) && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }

        public List<Sale> GetByUser(string userCuit, DateTime from, DateTime to)
        {
            return _context.Sales
                .Include(s => s.Details)
                .Where(s => s.UserCuit == userCuit && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }
    }
}