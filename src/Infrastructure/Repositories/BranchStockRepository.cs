using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class BranchStockRepository : Repository<BranchStock>, IBranchStockRepository
    {
        public BranchStockRepository(ApplicationContext context) : base(context)
        {
        }

        public List<BranchStock> GetByBranch(Guid branchId)
        {
            return _context.BranchStocks.Where(x => x.BranchId == branchId && x.IsActive).ToList();
        }

        public List<BranchStock> GetByBranchIncludingInactive(Guid branchId)
        {
            return _context.BranchStocks.Where(x => x.BranchId == branchId).ToList();
        }

        // Stock activo del producto en todas las sucursales
        public List<BranchStock> GetByProduct(Guid productId)
        {
            return _context.BranchStocks.Where(x => x.ProductId == productId && x.IsActive).ToList();
        }

        // Devuelve también el stock dado de baja, para poder reactivarlo
        public BranchStock? GetById(Guid branchId, Guid productId)
        {
            return _context.BranchStocks.FirstOrDefault(x => x.BranchId == branchId && x.ProductId == productId);
        }
    }
}