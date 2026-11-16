using Domain.Entities;

namespace Application.Interfaces
{
    // GetAll, Create, Update y Delete vienen de IRepository<BranchStock>
    public interface IBranchStockRepository : IRepository<BranchStock>
    {
        List<BranchStock> GetByBranch(Guid branchId);

        // Clave compuesta: sucursal + producto
        BranchStock? GetById(Guid branchId, Guid productId);
    }
}
