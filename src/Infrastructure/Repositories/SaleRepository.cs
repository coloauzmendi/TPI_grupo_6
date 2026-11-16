using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
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

        public Sale? GetById(Guid branchId, Guid saleId)
        {
            return _context.Sales
                .Include(s => s.Details)
                .FirstOrDefault(s => s.Id == saleId && s.BranchId == branchId);
        }

        public void Cancel(Sale sale, List<BranchStock> restoredStocks)
        {
            _context.Sales.Update(sale);
            _context.BranchStocks.UpdateRange(restoredStocks);
            _context.SaveChanges(); // una sola transacción: anulación + stock
        }

        public List<Sale> GetByBranches(List<Guid> branchIds, DateTime from, DateTime to)
        {
            return _context.Sales
                .Include(s => s.Details)
                .Where(s => !s.IsCancelled && branchIds.Contains(s.BranchId) && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }

        public List<Sale> GetByUser(string userCuit, DateTime from, DateTime to)
        {
            return _context.Sales
                .Include(s => s.Details)
                .Where(s => !s.IsCancelled && s.UserCuit == userCuit && s.Date >= from && s.Date < to)
                .OrderBy(s => s.Date)
                .ToList();
        }
    }
}