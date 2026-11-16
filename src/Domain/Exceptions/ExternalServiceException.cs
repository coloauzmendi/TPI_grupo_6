namespace Domain.Exceptions
{
    // Lanzada cuando un servicio de terceros no responde o devuelve datos inválidos (503)
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message)
            : base(message)
        {
        }

        public ExternalServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
