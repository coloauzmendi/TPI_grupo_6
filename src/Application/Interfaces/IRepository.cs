namespace Application.Interfaces
{
    // Repositorio genérico: las operaciones comunes a todas las entidades.
    // Cada repositorio específico hereda de esta interfaz y agrega solo lo propio de su entidad
    // (por ejemplo GetById, porque cada entidad tiene una clave distinta: Guid, CUIT o clave compuesta).
    public interface IRepository<T> where T : class
    {
        List<T> GetAll();
        T Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
