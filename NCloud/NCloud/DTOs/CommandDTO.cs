namespace NCloud.DTOs
{
    public class CommandDTO
    {
        public bool IsClientSide { get; set; } = false;
        public string ActionHTMLElement { get; set; } = String.Empty;
        public string ActionHTMLElementId { get; set; } = String.Empty;
        public bool NoErrorWithSyntax { get; set; } = false;
        public string ErrorMessage { get; set; } = String.Empty;
    }
}
