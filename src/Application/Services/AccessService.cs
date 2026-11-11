using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    // Decide a qué empresas y sucursales puede acceder cada usuario (según su CUIT del token)
    public class AccessService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ICompanyRepository _companyRepository;

        public AccessService(
            IUserRepository userRepository,
            IBranchRepository branchRepository,
            ICompanyRepository companyRepository)
        {
            _userRepository = userRepository;
            _branchRepository = branchRepository;
            _companyRepository = companyRepository;
        }

        // Un usuario dado de baja deja de tener acceso aunque su token siga vigente
        public User GetUser(string cuit)
        {
            User? user = _userRepository.GetByCuit(cuit);
            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedException("El usuario no existe o fue dado de baja.");
            }

            return user;
        }

        // Dueño: sus empresas. Empleado: la empresa de su sucursal.
        public List<Company> GetCompanies(string cuit)
        {
            User user = GetUser(cuit);
            if (user.Role == UserRole.Owner)
            {
                return _companyRepository.GetByOwner(cuit);
            }

            Branch? branch = user.BranchId is null ? null : _branchRepository.GetById(user.BranchId.Value);
            Company? company = branch is null ? null : _companyRepository.GetById(branch.CompanyId);

            return company is null ? new List<Company>() : new List<Company> { company };
        }

        // Dueño: las sucursales de sus empresas. Empleado: solo la suya.
        public List<Branch> GetBranches(string cuit)
        {
            User user = GetUser(cuit);
            if (user.Role == UserRole.Owner)
            {
                return GetCompanies(cuit)
                    .SelectMany(c => _branchRepository.GetByCompany(c.Id))
                    .ToList();
            }

            Branch? branch = user.BranchId is null ? null : _branchRepository.GetById(user.BranchId.Value);

            return branch is null ? new List<Branch>() : new List<Branch> { branch };
        }

        public Branch EnsureBranchAccess(string cuit, Guid branchId)
        {
            Branch? branch = GetBranches(cuit).FirstOrDefault(b => b.Id == branchId);
            if (branch is null)
            {
                throw new NotAllowedException("No tenés acceso a esa sucursal.");
            }

            return branch;
        }

        public Company EnsureCompanyOwner(string cuit, Guid companyId)
        {
            Company? company = _companyRepository.GetByOwner(cuit).FirstOrDefault(c => c.Id == companyId);
            if (company is null)
            {
                throw new NotAllowedException("No sos dueño de esa empresa.");
            }

            return company;
        }
    }
}