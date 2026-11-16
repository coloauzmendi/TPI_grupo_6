using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        List<Product> GetAllIncludingInactive();
        Product? GetById(Guid id);
        Product Create(Product newProduct);
        void Update(Product updatedProduct);
        void UpdateRange(List<Product> updatedProducts);
        void Delete(Product productToDelete, List<BranchStock> stocksToDelete);
    }
}