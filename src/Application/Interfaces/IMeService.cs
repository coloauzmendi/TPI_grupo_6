using Application.Models;

namespace Application.Interfaces
{
    public interface IMeService
    {
        MeResponse GetMe(string cuit);
        List<BranchResponse> GetBranches(string cuit);
        List<CompanyDetailResponse> GetCompanies(string cuit);
        List<StockAlertResponse> GetStockAlerts(string cuit);
        List<EmployeeResponse> GetEmployees(string cuit);
    }
}
