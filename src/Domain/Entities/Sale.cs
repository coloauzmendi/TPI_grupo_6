namespace Domain.Entities
{
    public class Sale
    {
        public Guid Id { get; set; }

        // Número correlativo que se muestra como V-000123
        public int Number { get; set; }

        public Guid BranchId { get; set; }

        // CUIT del empleado que registró la venta
        public string UserCuit { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Total { get; set; }

        public List<SaleDetail> Details { get; set; } = new();
    }
}