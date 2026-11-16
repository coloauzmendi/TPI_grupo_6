namespace Domain.Interfaces
{
    // Entidades con baja lógica
    public interface ISoftDelete
    {
        bool IsActive { get; set; }
    }
}
