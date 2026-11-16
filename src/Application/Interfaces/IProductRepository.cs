using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        List<Product> GetAll();
        List<Product> GetAllIncludingInactive();
        List<Product> GetByCompanies(List<Guid> companyIds);
        List<Product> GetByCompanyIncludingInactive(Guid companyId);
        Product? GetById(Guid id);
        void UpdateRange(List<Product> updatedProducts);
        void Delete(Product productToDelete, List<BranchStock> stocksToDelete);
    }
}