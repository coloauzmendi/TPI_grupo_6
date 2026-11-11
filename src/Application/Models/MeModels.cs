using Domain.Enums;

namespace Application.Models
{
    public class BranchResponse
    {
        public Guid Id { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public class MeResponse
    {
        public string Cuit { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        // Solo para Employee
        public BranchResponse? Branch { get; set; }
    }

    public class OwnerSummary
    {
        public string Cuit { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class BranchDetailResponse
    {
        public Guid Id { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class CompanyDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<OwnerSummary> Owners { get; set; } = new();
        public List<BranchDetailResponse> Branches { get; set; } = new();
    }

    public class EmployeeResponse
    {
        public string Cuit { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        // Ventas registradas en el mes actual
        public int MonthSales { get; set; }
    }
}