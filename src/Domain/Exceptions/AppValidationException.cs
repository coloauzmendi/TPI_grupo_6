namespace Domain.Exceptions
{
    // Lanzada por la capa Application: validaciones de reglas de negocio o de formato de los datos (400)
    public class AppValidationException : Exception
    {
        public AppValidationException()
            : base()
        {
        }

        public AppValidationException(string message)
            : base(message)
        {
        }

        public AppValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
