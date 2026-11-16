using Application.Models;

namespace Application.Interfaces
{
    public interface IReportService
    {
        TodayResponse GetToday(string cuit);
        RankingResponse GetRanking(string cuit, Guid? companyId, Guid? branchId, int year, int month, bool mostSold);
        Task<MonthlyCloseResponse> GetMonthlyCloseAsync(string cuit, Guid? companyId, Guid? branchId, int year, int month);
    }
}
