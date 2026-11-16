using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repositories
{
    // Create y Update se heredan de Repository<Company>
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationContext context) : base(context)
        {
        }

        // Solo las empresas activas
        public override List<Company> GetAll()
        {
            return _dbSet.Where(x => x.IsActive).ToList();
        }

        public Company? GetById(Guid id)
        {
            return _dbSet.FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public List<Company> GetByOwner(string ownerCuit)
        {
            return _dbSet
                .Where(c => c.IsActive && c.Owners.Any(o => o.Cuit == ownerCuit))
                .ToList();
        }

        // Baja lógica
        public override void Delete(Company companyToDelete)
        {
            companyToDelete.IsActive = false;
            Update(companyToDelete);
        }

        public List<User> GetOwners(Guid companyId)
        {
            return _dbSet
                .Where(c => c.Id == companyId)
                .SelectMany(c => c.Owners)
                .Where(o => o.IsActive)
                .ToList();
        }

        public void AddOwner(Guid companyId, User owner)
        {
            var company = _dbSet
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
            var company = _dbSet
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
