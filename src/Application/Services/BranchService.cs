using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

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
        public List<Branch> GetAll(string cuit)
        {
            return _accessService.GetBranches(cuit);
        }

        public Branch GetById(string cuit, Guid id)
        {
            return _accessService.EnsureBranchAccess(cuit, id);
        }

        // La sucursal se crea en una empresa de la que el usuario es dueño
        public Branch Create(string cuit, Branch newBranch)
        {
            Validate(newBranch);
            _accessService.EnsureCompanyOwner(cuit, newBranch.CompanyId);

            newBranch.Id = Guid.NewGuid();
            newBranch.IsActive = true;

            return _branchRepository.Create(newBranch);
        }

        // También permite moverla a otra empresa, siempre que el usuario sea dueño de ambas
        public void Update(string cuit, Guid id, Branch updatedBranch)
        {
            Validate(updatedBranch);
            Branch branch = _accessService.EnsureBranchAccess(cuit, id);
            _accessService.EnsureCompanyOwner(cuit, updatedBranch.CompanyId);

            branch.Locality = updatedBranch.Locality.Trim();
            branch.Address = updatedBranch.Address.Trim();
            branch.Industry = updatedBranch.Industry;
            branch.CompanyId = updatedBranch.CompanyId;

            _branchRepository.Update(branch);
        }

        // Baja lógica: también se dan de baja los empleados asignados a la sucursal
        public void Delete(string cuit, Guid id)
        {
            Branch branch = _accessService.EnsureBranchAccess(cuit, id);

            foreach (User employee in _userRepository.GetByBranch(id))
            {
                _userRepository.Delete(employee);
            }

            _branchRepository.Delete(branch);
        }

        private static void Validate(Branch branch)
        {
            if (string.IsNullOrWhiteSpace(branch.Locality) || string.IsNullOrWhiteSpace(branch.Address))
            {
                throw new AppValidationException("La localidad y la dirección de la sucursal son obligatorias.");
            }
        }
    }
}
