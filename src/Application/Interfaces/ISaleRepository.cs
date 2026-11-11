using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISaleRepository
    {
        // Guarda la venta y el stock descontado en una única transacción
        Sale Create(Sale newSale, List<BranchStock> updatedStocks);

        // Ventas de las sucursales indicadas entre dos fechas (from inclusive, to exclusive), con su detalle
        List<Sale> GetByBranches(List<Guid> branchIds, DateTime from, DateTime to);

        List<Sale> GetByUser(string userCuit, DateTime from, DateTime to);
    }
}