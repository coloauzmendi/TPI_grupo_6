using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICompanyRepository
    {
        List<Company> GetAll();
        Company? GetById(Guid id);
        Company Create(Company newCompany);
        void Update(Company updatedCompany);
        void Delete(Company companyToDelete);

        List<User> GetOwners(Guid companyId);
        void AddOwner(Guid companyId, User owner);
        void RemoveOwner(Guid companyId, string ownerCuit);
    }
}
