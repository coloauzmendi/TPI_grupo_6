using Application.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchService
    {
        List<BranchResponse> GetAll(string cuit);
        BranchResponse GetById(string cuit, Guid id);
        BranchResponse Create(string cuit, BranchRequest request);
        void Update(string cuit, Guid id, BranchRequest request);
        void Delete(string cuit, Guid id);
        void Deactivate(Branch branch);
    }
}
