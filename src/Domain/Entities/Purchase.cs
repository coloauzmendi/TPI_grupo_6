namespace Domain.Entities
{
    public class Purchase
    {
        public Guid Id { get; set; }

        // Número correlativo que se muestra como C-00012
        public int Number { get; set; }

        public Guid BranchId { get; set; }

        public string UserCuit { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string? Supplier { get; set; }

        public decimal Total { get; set; }

        public List<PurchaseDetail> Details { get; set; } = new();
    }
}