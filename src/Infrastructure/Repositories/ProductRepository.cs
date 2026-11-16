using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    // GetAll, Create, Update y Delete se heredan de Repository<Product>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationContext context) : base(context)
        {
        }

        public Product? GetById(Guid id)
        {
            return _dbSet.FirstOrDefault(x => x.Id == id);
        }

        // Actualiza varios productos en una sola transacción (actualización masiva de precios)
        public void UpdateRange(List<Product> updatedProducts)
        {
            _dbSet.UpdateRange(updatedProducts);
            _context.SaveChanges();
        }

        public bool HasMovements(Guid productId)
        {
            return _context.SaleDetails.Any(d => d.ProductId == productId)
                || _context.PurchaseDetails.Any(d => d.ProductId == productId);
        }
    }
}
