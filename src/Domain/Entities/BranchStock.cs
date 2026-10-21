namespace Domain.Entities
{
    public class BranchStock
    {
        public Guid BranchId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public int MinimumStock { get; set; }
    }
}
