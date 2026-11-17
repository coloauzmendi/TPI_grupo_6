using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    public class BranchRequest
    {
        [Required(ErrorMessage = "La localidad es obligatoria.")]
        [StringLength(100, ErrorMessage = "La localidad no puede superar los 100 caracteres.")]
        public string Locality { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(150, ErrorMessage = "La dirección no puede superar los 150 caracteres.")]
        public string Address { get; set; } = string.Empty;

        public Industry Industry { get; set; }

        public Guid CompanyId { get; set; }
    }

    public class BranchResponse
    {
        public Guid Id { get; set; }
        public string Locality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
