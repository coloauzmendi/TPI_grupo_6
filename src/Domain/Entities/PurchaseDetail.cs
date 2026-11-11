namespace Domain.Entities
{
    public class PurchaseDetail
    {
        public Guid PurchaseId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }
}