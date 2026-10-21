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
            return _context.Products.ToList();
        }

        public Product? GetById(Guid id)
        {
            return _context.Products.FirstOrDefault(x => x.Id == id);
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

        public void Delete(Product productToDelete)
        {
            _context.Products.Remove(productToDelete);
            _context.SaveChanges();
        }
    }
}
