using Application.Models;

namespace Application.Interfaces
{
    public interface ISaleService
    {
        SaleResultResponse Create(string cuit, Guid branchId, SaleRequest request);
    }
}
