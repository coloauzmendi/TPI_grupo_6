using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchStockRepository
    {
        List<BranchStock> GetByBranch(Guid branchId);
        List<BranchStock> GetByBranchIncludingInactive(Guid branchId);
        List<BranchStock> GetByProduct(Guid productId);
        BranchStock? GetById(Guid branchId, Guid productId);
        BranchStock Create(BranchStock newStock);
        void Update(BranchStock updatedStock);
        void Delete(BranchStock stockToDelete);
    }
}
