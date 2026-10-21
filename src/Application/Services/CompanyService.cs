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

        public CompanyService(
            ICompanyRepository companyRepository,
            IUserRepository userRepository,
            IBranchRepository branchRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _branchRepository = branchRepository;
        }

        public List<Company> GetAll()
        {
            return _companyRepository.GetAll();
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

        public Company Create(CompanyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new AppValidationException("El nombre de la empresa es obligatorio.");
            }

            Company newCompany = new Company
            {
                Name = request.Name
            };

            return _companyRepository.Create(newCompany);
        }

        public void Update(Guid id, CompanyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new AppValidationException("El nombre de la empresa es obligatorio.");
            }

            Company company = GetById(id);
            company.Name = request.Name;

            _companyRepository.Update(company);
        }

        public void Delete(Guid id)
        {
            Company company = GetById(id);

            // Al borrar la empresa se borran sus sucursales, así que no pueden tener empleados
            List<Branch> branches = _branchRepository.GetByCompany(id);
            if (branches.Any(b => _userRepository.GetByBranch(b.Id).Count > 0))
            {
                throw new AppValidationException("La empresa tiene sucursales con empleados asignados.");
            }

            _companyRepository.Delete(company);
        }

        // Dueños de la empresa

        public List<User> GetOwners(Guid companyId)
        {
            GetById(companyId);

            return _companyRepository.GetOwners(companyId);
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

            _companyRepository.RemoveOwner(companyId, cuit);
        }
    }
}
