using Domain.Enums;

namespace Application.Models
{
    // Mode = Individual: Target es el Id del producto y Value el precio nuevo.
    // Mode = Category / Industry: Target es la categoría o el rubro y Value un porcentaje (negativo = descuento).
    // Mode = All: sin Target, Value es un porcentaje.
    public class PriceUpdateRequest
    {
        public PriceMode Mode { get; set; }

        public string? Target { get; set; }

        public decimal Value { get; set; }
    }
}