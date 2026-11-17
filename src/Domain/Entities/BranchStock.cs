using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class BranchStock : ISoftDelete
    {
        public Guid BranchId { get; set; }

        public Guid ProductId { get; set; }

        // Solo cambia a través de SetQuantity, Add y Remove: nunca puede quedar negativo
        public int Quantity { get; private set; }

        public int MinimumStock { get; set; }

        // Baja lógica: el producto deja de estar activo en la sucursal
        public bool IsActive { get; set; } = true;

        public bool IsLow => Quantity <= MinimumStock;

        public void SetQuantity(int quantity)
        {
            if (quantity < 0)
            {
                throw new AppValidationException("El stock no puede ser negativo.");
            }

            Quantity = quantity;
        }

        public void Add(int quantity)
        {
            EnsurePositive(quantity);
            Quantity += quantity;
        }

        public void Remove(int quantity)
        {
            EnsurePositive(quantity);
            if (quantity > Quantity)
            {
                throw new AppValidationException("Stock insuficiente.");
            }

            Quantity -= quantity;
        }

        private static void EnsurePositive(int quantity)
        {
            if (quantity <= 0)
            {
                throw new AppValidationException("La cantidad debe ser mayor a 0.");
            }
        }
    }
}