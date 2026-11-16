using Application.Models;

namespace Application.Interfaces
{
    public interface IPurchaseService
    {
        PurchaseResponse Create(string cuit, Guid branchId, PurchaseRequest request);
    }
}
