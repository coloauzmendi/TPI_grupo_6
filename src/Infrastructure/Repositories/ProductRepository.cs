using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationContext context) : base(context)
        {
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

        public List<Product> GetByCompanies(List<Guid> companyIds)
        {
            return _context.Products.Where(x => x.IsActive && companyIds.Contains(x.CompanyId)).ToList();
        }

        // Incluye los dados de baja para poder reactivarlos en el alta
        public List<Product> GetByCompanyIncludingInactive(Guid companyId)
        {
            return _context.Products.Where(x => x.CompanyId == companyId).ToList();
        }

        public Product? GetById(Guid id)
        {
            return _context.Products.FirstOrDefault(x => x.Id == id && x.IsActive);
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