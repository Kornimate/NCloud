namespace NCloud.DTOs
{
    public class ConnectionDTO
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = String.Empty;
        public string Result { get; set; } = String.Empty;
        public string Payload { get; set; } = String.Empty;
    }
}
