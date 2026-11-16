using Domain.Entities;

namespace Application.Interfaces
{
    // GetAll, Create, Update y Delete vienen de IRepository<Product>
    public interface IProductRepository : IRepository<Product>
    {
        Product? GetById(Guid id);
        void UpdateRange(List<Product> updatedProducts);

        // true si el producto aparece en alguna venta o compra (no se puede borrar sin perder el historial)
        bool HasMovements(Guid productId);
    }
}
