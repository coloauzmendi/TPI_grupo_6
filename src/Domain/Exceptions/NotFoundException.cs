namespace Domain.Exceptions
{
    // El recurso buscado no existe (404)
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string name, object key)
            : base($"La entidad {name} ({key}) no existe.")
        {
        }
    }
}
