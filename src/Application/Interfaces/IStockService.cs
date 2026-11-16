using Application.Models;

namespace Application.Interfaces
{
    public interface IStockService
    {
        List<StockItemResponse> GetItems(string cuit, Guid branchId, bool includeInactive = false);
        BranchStockResponse GetById(string cuit, Guid branchId, Guid productId);
        BranchStockResponse Create(string cuit, Guid branchId, BranchStockCreateRequest request);
        void Update(string cuit, Guid branchId, Guid productId, BranchStockUpdateRequest request);
        void Delete(string cuit, Guid branchId, Guid productId);
    }
}
