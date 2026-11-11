using Domain.Enums;

namespace Application.Models
{
    public class KpiResponse
    {
        public int Sales { get; set; }
        public int Units { get; set; }
        public decimal Total { get; set; }
    }

    public class TodayResponse
    {
        public KpiResponse Kpi { get; set; } = new();
        // Últimas ventas del día, la más reciente primero
        public List<SaleResponse> Recent { get; set; } = new();
    }

    public class RankingRowResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }
    }

    public class RankingResponse
    {
        public KpiResponse Kpi { get; set; } = new();
        public List<RankingRowResponse> Rows { get; set; } = new();
    }

    public class MonthTotalResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
    }

    public class TopProductResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Units { get; set; }
    }

    // Lo vendido de un producto en los 12 meses del gráfico de barras
    public class BreakdownRowResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }
    }

    public class BranchClosingResponse
    {
        public Guid BranchId { get; set; }
        public int Sales { get; set; }
        public int Units { get; set; }
        public decimal Total { get; set; }
    }

    public class MonthlyCloseResponse
    {
        public KpiResponse Current { get; set; } = new();
        public KpiResponse Previous { get; set; } = new();
        public KpiResponse LastYear { get; set; } = new();
        // Los 12 meses que terminan en el mes pedido
        public List<MonthTotalResponse> Months { get; set; } = new();
        public List<TopProductResponse> Top { get; set; } = new();
        // Composición de las ventas de esos mismos 12 meses, por producto
        public List<BreakdownRowResponse> Breakdown { get; set; } = new();
        public List<BranchClosingResponse> Branches { get; set; } = new();
    }
}