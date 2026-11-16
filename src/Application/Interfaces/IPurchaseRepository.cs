using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPurchaseRepository
    {
        // Guarda la compra y el stock incrementado en una única transacción
        Purchase Create(Purchase newPurchase, List<BranchStock> updatedStocks);

        // Costo unitario de la compra más reciente de cada producto en las sucursales indicadas
        Dictionary<Guid, decimal> GetLatestUnitCosts(List<Guid> branchIds);
    }
}