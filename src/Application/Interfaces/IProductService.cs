using Application.Models;

namespace Application.Interfaces
{
    public interface IProductService
    {
        // Productos de las empresas a las que accede el usuario; companyId limita a una de ellas
        List<ProductResponse> GetAll(string cuit, Guid? companyId = null);
        ProductResponse GetById(string cuit, Guid id);
        ProductResponse Create(string cuit, ProductCreateRequest request);
        void Update(string cuit, Guid id, ProductRequest request);
        void Delete(string cuit, Guid id);
    }
}
