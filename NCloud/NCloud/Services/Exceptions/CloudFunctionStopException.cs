namespace NCloud.Services.Exceptions
{
    /// <summary>
    /// Exception to handle error where execution must be stopped
    /// </summary>
    public class CloudFunctionStopException : Exception
    {
        public CloudFunctionStopException() : base() { }
        public CloudFunctionStopException(string message) : base(message) { }
    }
}
