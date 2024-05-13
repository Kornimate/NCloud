namespace NCloud.Services.Exceptions
{
    public class CloudFunctionStopException : Exception
    {
        public CloudFunctionStopException() : base() { }
        public CloudFunctionStopException(string message) : base(message) { }
    }
}
