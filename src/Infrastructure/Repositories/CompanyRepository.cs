using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationContext _context;

        public CompanyRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<Company> GetAll()
        {
            return _context.Companies.ToList();
        }

        public Company? GetById(Guid id)
        {
            return _context.Companies.FirstOrDefault(x => x.Id == id);
        }

        public Company Create(Company newCompany)
        {
            _context.Companies.Add(newCompany);
            _context.SaveChanges();

            return newCompany;
        }

        public void Update(Company updatedCompany)
        {
            _context.Companies.Update(updatedCompany);
            _context.SaveChanges();
        }

        public void Delete(Company companyToDelete)
        {
            _context.Companies.Remove(companyToDelete);
            _context.SaveChanges();
        }

        public List<User> GetOwners(Guid companyId)
        {
            return _context.Companies
                .Where(c => c.Id == companyId)
                .SelectMany(c => c.Owners)
                .ToList();
        }

        public void AddOwner(Guid companyId, User owner)
        {
            var company = _context.Companies
                .Include(c => c.Owners)
                .First(c => c.Id == companyId);

            if (!company.Owners.Any(o => o.Cuit == owner.Cuit))
            {
                company.Owners.Add(owner);
                _context.SaveChanges();
            }
        }

        public void RemoveOwner(Guid companyId, string ownerCuit)
        {
            var company = _context.Companies
                .Include(c => c.Owners)
                .First(c => c.Id == companyId);

            var owner = company.Owners.FirstOrDefault(o => o.Cuit == ownerCuit);
            if (owner is not null)
            {
                company.Owners.Remove(owner);
                _context.SaveChanges();
            }
        }
    }
}
