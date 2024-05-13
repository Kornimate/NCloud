namespace NCloud.Services.Exceptions
{
    public class CloudLoggerException : Exception
    {
        public CloudLoggerException() : base() { }
        public CloudLoggerException(string message) : base(message) { }
    }
}
