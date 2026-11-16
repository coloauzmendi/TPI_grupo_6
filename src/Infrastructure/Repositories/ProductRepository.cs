using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationContext _context;

        public ProductRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<Product> GetAll()
        {
            return _context.Products.Where(x => x.IsActive).ToList();
        }

        // Incluye los dados de baja para resolver nombres en ventas y compras anteriores
        public List<Product> GetAllIncludingInactive()
        {
            return _context.Products.ToList();
        }

        public Product? GetById(Guid id)
        {
            return _context.Products.FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public Product Create(Product newProduct)
        {
            _context.Products.Add(newProduct);
            _context.SaveChanges();

            return newProduct;
        }

        public void Update(Product updatedProduct)
        {
            _context.Products.Update(updatedProduct);
            _context.SaveChanges();
        }

        // Actualiza varios productos en una sola transacción (actualización masiva de precios)
        public void UpdateRange(List<Product> updatedProducts)
        {
            _context.Products.UpdateRange(updatedProducts);
            _context.SaveChanges();
        }

        public void Delete(Product productToDelete, List<BranchStock> stocksToDelete)
        {
            productToDelete.IsActive = false;
            stocksToDelete.ForEach(s => s.IsActive = false);

            _context.Products.Update(productToDelete);
            _context.BranchStocks.UpdateRange(stocksToDelete);
            _context.SaveChanges(); // una sola transacción: producto + stock
        }
    }
}