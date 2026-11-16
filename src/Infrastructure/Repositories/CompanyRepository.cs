using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationContext context) : base(context)
        {
        }

        public Company? GetById(Guid id)
        {
            return _context.Companies.FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public List<Company> GetByOwner(string ownerCuit)
        {
            return _context.Companies
                .Where(c => c.IsActive && c.Owners.Any(o => o.Cuit == ownerCuit))
                .ToList();
        }

        public List<User> GetOwners(Guid companyId)
        {
            return _context.Companies
                .Where(c => c.Id == companyId)
                .SelectMany(c => c.Owners)
                .Where(o => o.IsActive)
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
