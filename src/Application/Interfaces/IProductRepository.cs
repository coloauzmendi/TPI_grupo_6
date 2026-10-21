using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        Product? GetById(Guid id);
        Product Create(Product newProduct);
        void Update(Product updatedProduct);
        void Delete(Product productToDelete);
    }
}
