namespace Application.Models
{
    public class PurchaseItemRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }

    public class PurchaseRequest
    {
        public string? Supplier { get; set; }

        public List<PurchaseItemRequest> Items { get; set; } = new();
    }

    public class PurchaseItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        // Stock de la sucursal después de ingresar la mercadería
        public int NewStock { get; set; }
    }

    public class PurchaseResponse
    {
        public Guid Id { get; set; }
        // Número formateado: C-00012
        public string Code { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public decimal Total { get; set; }
        public List<PurchaseItemResponse> Items { get; set; } = new();
    }
}