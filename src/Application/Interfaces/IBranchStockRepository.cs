using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchStockRepository : IRepository<BranchStock>
    {
        List<BranchStock> GetByBranch(Guid branchId);
        List<BranchStock> GetByBranchIncludingInactive(Guid branchId);
        List<BranchStock> GetByProduct(Guid productId);
        BranchStock? GetById(Guid branchId, Guid productId);
    }
}
