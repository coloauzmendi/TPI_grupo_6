namespace Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T Create(T entity);
        void Update(T entity);
        // Baja lógica si la entidad la admite; si no, se elimina
        void Delete(T entity);
    }
}
