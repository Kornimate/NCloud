namespace NCloud.DTOs
{
    /// <summary>
    /// Class to wrap result of action execution and return it to JS calls (command and connection)
    /// </summary>
    public class ConnectionDTO
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = String.Empty;
        public string Result { get; set; } = String.Empty;
        public string Payload { get; set; } = String.Empty;
        public string Redirection { get; set; } = String.Empty;
    }
}
