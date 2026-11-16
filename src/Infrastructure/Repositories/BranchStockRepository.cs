using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    // Create y Update se heredan de Repository<BranchStock>
    public class BranchStockRepository : Repository<BranchStock>, IBranchStockRepository
    {
        public BranchStockRepository(ApplicationContext context) : base(context)
        {
        }

        // Solo el stock activo
        public override List<BranchStock> GetAll()
        {
            return _dbSet.Where(x => x.IsActive).ToList();
        }

        public List<BranchStock> GetByBranch(Guid branchId)
        {
            return _dbSet.Where(x => x.BranchId == branchId && x.IsActive).ToList();
        }

        // Devuelve también el stock dado de baja, para poder reactivarlo
        public BranchStock? GetById(Guid branchId, Guid productId)
        {
            return _dbSet.FirstOrDefault(x => x.BranchId == branchId && x.ProductId == productId);
        }

        // Baja lógica: el producto deja de estar activo en la sucursal
        public override void Delete(BranchStock stockToDelete)
        {
            stockToDelete.IsActive = false;
            Update(stockToDelete);
        }
    }
}
