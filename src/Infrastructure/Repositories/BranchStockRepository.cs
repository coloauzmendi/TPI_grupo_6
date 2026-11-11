using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    public class BranchStockRepository : IBranchStockRepository
    {
        private readonly ApplicationContext _context;

        public BranchStockRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<BranchStock> GetByBranch(Guid branchId)
        {
            return _context.BranchStocks.Where(x => x.BranchId == branchId && x.IsActive).ToList();
        }

        // Devuelve también el stock dado de baja, para poder reactivarlo
        public BranchStock? GetById(Guid branchId, Guid productId)
        {
            return _context.BranchStocks.FirstOrDefault(x => x.BranchId == branchId && x.ProductId == productId);
        }

        public BranchStock Create(BranchStock newStock)
        {
            _context.BranchStocks.Add(newStock);
            _context.SaveChanges();

            return newStock;
        }

        public void Update(BranchStock updatedStock)
        {
            _context.BranchStocks.Update(updatedStock);
            _context.SaveChanges();
        }

        // Baja lógica
        public void Delete(BranchStock stockToDelete)
        {
            stockToDelete.IsActive = false;
            _context.BranchStocks.Update(stockToDelete);
            _context.SaveChanges();
        }
    }
}