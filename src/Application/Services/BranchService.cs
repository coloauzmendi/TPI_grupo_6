using Application.Interfaces;
using Application.Models;
using Domain.Entities;

namespace Application.Services
{
    public class BranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IUserRepository _userRepository;
        private readonly AccessService _accessService;

        public BranchService(
            IBranchRepository branchRepository,
            IUserRepository userRepository,
            AccessService accessService)
        {
            _branchRepository = branchRepository;
            _userRepository = userRepository;
            _accessService = accessService;
        }

        // Dueño: las sucursales de sus empresas. Empleado: solo la suya.
        public List<BranchResponse> GetAll(string cuit)
        {
            Dictionary<Guid, string> companyNames = CompanyNames(cuit);

            return _accessService.GetBranches(cuit)
                .Select(b => ResponseMapper.ToResponse(b, companyNames.GetValueOrDefault(b.CompanyId, string.Empty)))
                .ToList();
        }

        public BranchResponse GetById(string cuit, Guid id)
        {
            Branch branch = _accessService.EnsureBranchAccess(cuit, id);

            return ResponseMapper.ToResponse(branch, CompanyNames(cuit).GetValueOrDefault(branch.CompanyId, string.Empty));
        }

        public BranchResponse Create(string cuit, BranchRequest request)
        {
            Company company = _accessService.EnsureCompanyOwner(cuit, request.CompanyId);

            Branch created = _branchRepository.Create(new Branch
            {
                Id = Guid.NewGuid(),
                Locality = request.Locality,
                Address = request.Address,
                Industry = request.Industry,
                CompanyId = request.CompanyId
            });

            return ResponseMapper.ToResponse(created, company.Name);
        }

        public void Update(string cuit, Guid id, BranchRequest request)
        {
            Branch branch = _accessService.EnsureBranchAccess(cuit, id);
            _accessService.EnsureCompanyOwner(cuit, request.CompanyId);

            branch.Locality = request.Locality;
            branch.Address = request.Address;
            branch.Industry = request.Industry;
            branch.CompanyId = request.CompanyId;
            _branchRepository.Update(branch);
        }

        public void Delete(string cuit, Guid id)
        {
            Deactivate(_accessService.EnsureBranchAccess(cuit, id));
        }

        // Baja lógica: también se dan de baja los empleados asignados a la sucursal
        public void Deactivate(Branch branch)
        {
            foreach (User employee in _userRepository.GetByBranch(branch.Id))
            {
                _userRepository.Delete(employee);
            }

            _branchRepository.Delete(branch);
        }

        private Dictionary<Guid, string> CompanyNames(string cuit)
        {
            return _accessService.GetCompanies(cuit).ToDictionary(c => c.Id, c => c.Name);
        }
    }
}
