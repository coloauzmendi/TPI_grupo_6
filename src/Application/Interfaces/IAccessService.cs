using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAccessService
    {
        User GetUser(string cuit);
        List<Company> GetCompanies(string cuit);
        List<Branch> GetBranches(string cuit);
        Branch EnsureBranchAccess(string cuit, Guid branchId);
        Company EnsureCompanyOwner(string cuit, Guid companyId);
    }
}
