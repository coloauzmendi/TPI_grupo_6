using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    // Mode = Individual: Target es el Id del producto y Value el precio nuevo.
    // Mode = Category / Industry: Target es la categoría o el rubro y Value un porcentaje (negativo = descuento).
    // Mode = All: sin Target, Value es un porcentaje.
    public class PriceUpdateRequest
    {
        public PriceMode Mode { get; set; }

        [StringLength(50, ErrorMessage = "El destino no puede superar los 50 caracteres.")]
        public string? Target { get; set; }

        [Range(typeof(decimal), "-99", ValidationLimits.MaxAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El valor debe estar entre -99 y 99.999.999,99.")]
        public decimal Value { get; set; }
    }
}
