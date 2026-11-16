namespace Domain.Exceptions
{
    // El recurso ya existe o choca con el estado actual (409); ej.: un CUIT ya registrado
    public class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
