namespace Domain.Exceptions
{
    // El recurso ya existe o choca con otro (409)
    public class ConflictException : Exception
    {
        public ConflictException()
            : base()
        {
        }

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
