namespace NCloud.Services.Exceptions
{
    /// <summary>
    /// Exception where execution should log info into the log file
    /// </summary>
    public class CloudLoggerException : Exception
    {
        public CloudLoggerException() : base() { }
        public CloudLoggerException(string message) : base(message) { }
    }
}
