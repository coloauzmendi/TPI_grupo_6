using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class CompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly BranchService _branchService;
        private readonly AccessService _accessService;

        public CompanyService(
            ICompanyRepository companyRepository,
            IUserRepository userRepository,
            IBranchRepository branchRepository,
            BranchService branchService,
            AccessService accessService)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _branchRepository = branchRepository;
            _branchService = branchService;
            _accessService = accessService;
        }

        public List<Company> GetAll()
        {
            return _companyRepository.GetAll();
        }

        // Solo las empresas de las que el usuario es dueño
        public List<CompanyResponse> GetByOwner(string cuit)
        {
            return _accessService.GetCompanies(cuit).Select(ResponseMapper.ToResponse).ToList();
        }

        public CompanyResponse GetByOwner(string cuit, Guid id)
        {
            return ResponseMapper.ToResponse(_accessService.EnsureCompanyOwner(cuit, id));
        }

        public Company GetById(Guid id)
        {
            Company? company = _companyRepository.GetById(id);
            if (company is null)
            {
                throw new NotFoundException("La empresa no existe.");
            }

            return company;
        }

        // El usuario que crea la empresa queda como su dueño
        public CompanyResponse Create(CompanyRequest request, string creatorCuit)
        {
            Company newCompany = new Company
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            Company created = _companyRepository.Create(newCompany);
            AddOwner(created.Id, creatorCuit);

            return ResponseMapper.ToResponse(created);
        }

        public void Update(Guid id, CompanyRequest request)
        {
            Company company = GetById(id);
            company.Name = request.Name;

            _companyRepository.Update(company);
        }

        // Baja lógica: también se dan de baja las sucursales y sus empleados
        public void Delete(Guid id)
        {
            Company company = GetById(id);

            foreach (Branch branch in _branchRepository.GetByCompany(id))
            {
                _branchService.Deactivate(branch);
            }

            _companyRepository.Delete(company);
        }
        // Dueños de la empresa

        public List<UserResponse> GetOwners(Guid companyId)
        {
            GetById(companyId);

            return _companyRepository.GetOwners(companyId).Select(ResponseMapper.ToResponse).ToList();
        }

        public void AddOwner(Guid companyId, string cuit)
        {
            GetById(companyId);

            User? user = _userRepository.GetByCuit(cuit);
            if (user is null)
            {
                throw new NotFoundException("El usuario no existe.");
            }

            if (user.Role != UserRole.Owner)
            {
                throw new AppValidationException("Solo un usuario Owner puede ser dueño de una empresa.");
            }

            _companyRepository.AddOwner(companyId, user);
        }

        public void RemoveOwner(Guid companyId, string cuit)
        {
            GetById(companyId);

            List<User> owners = _companyRepository.GetOwners(companyId);
            if (owners.Any(o => o.Cuit == cuit) && owners.All(o => o.Cuit == cuit))
            {
                throw new AppValidationException("La empresa debe tener al menos un dueño activo.");
            }

            _companyRepository.RemoveOwner(companyId, cuit);
        }
    }
}
