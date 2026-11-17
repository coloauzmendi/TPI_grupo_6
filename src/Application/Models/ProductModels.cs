using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    public class ProductRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
        public string Category { get; set; } = string.Empty;

        [Range(typeof(decimal), ValidationLimits.MinAmountText, ValidationLimits.MaxAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El precio debe estar entre 0,01 y 99.999.999,99.")]
        public decimal Price { get; set; }

        public Industry Industry { get; set; }
    }

    // La empresa solo se indica en el alta: un producto no puede cambiar de empresa
    public class ProductCreateRequest : ProductRequest
    {
        [Required(ErrorMessage = "La empresa es obligatoria.")]
        public Guid? CompanyId { get; set; }
    }

    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Industry Industry { get; set; }
        public Guid CompanyId { get; set; }
    }
}
