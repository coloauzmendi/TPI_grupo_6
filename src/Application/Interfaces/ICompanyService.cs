using Application.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICompanyService
    {
        List<CompanyResponse> GetByOwner(string cuit);
        CompanyResponse GetByOwner(string cuit, Guid id);
        Company GetById(Guid id);
        CompanyResponse Create(CompanyRequest request, string creatorCuit);
        void Update(Guid id, CompanyRequest request);
        void Delete(Guid id);
        List<UserResponse> GetOwners(Guid companyId);
        void AddOwner(Guid companyId, string cuit);
        void RemoveOwner(Guid companyId, string cuit);
    }
}
