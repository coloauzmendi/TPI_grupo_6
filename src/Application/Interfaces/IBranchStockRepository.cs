using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchStockRepository
    {
        List<BranchStock> GetByBranch(Guid branchId);
        BranchStock? GetById(Guid branchId, Guid productId);
        BranchStock Create(BranchStock newStock);
        void Update(BranchStock updatedStock);
        void Delete(BranchStock stockToDelete);
    }
}
