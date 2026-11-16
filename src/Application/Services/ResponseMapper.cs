using Application.Models;
using Domain.Entities;

namespace Application.Services
{
    // Arma las respuestas de la API a partir de las entidades
    public static class ResponseMapper
    {
        public static ProductResponse ToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Industry = product.Industry,
                CompanyId = product.CompanyId
            };
        }

        public static BranchResponse ToResponse(Branch branch, string companyName)
        {
            return new BranchResponse
            {
                Id = branch.Id,
                Locality = branch.Locality,
                Address = branch.Address,
                Industry = branch.Industry,
                CompanyId = branch.CompanyId,
                CompanyName = companyName
            };
        }

        public static BranchStockResponse ToResponse(BranchStock stock)
        {
            return new BranchStockResponse
            {
                BranchId = stock.BranchId,
                ProductId = stock.ProductId,
                Quantity = stock.Quantity,
                MinimumStock = stock.MinimumStock
            };
        }

        public static CompanyResponse ToResponse(Company company)
        {
            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name
            };
        }

        public static UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                Cuit = user.Cuit,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                BranchId = user.BranchId
            };
        }
    }
}
