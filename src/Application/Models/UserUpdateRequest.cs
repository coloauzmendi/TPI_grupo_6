namespace Application.Models
{
    // Edición de un empleado: la contraseña es opcional (si viene vacía no se cambia)
    public class UserUpdateRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }

        public string? Password { get; set; }
    }
}