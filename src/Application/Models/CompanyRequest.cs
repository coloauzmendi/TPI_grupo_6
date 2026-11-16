using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CompanyRequest
    {
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede superar los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;
    }

    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
