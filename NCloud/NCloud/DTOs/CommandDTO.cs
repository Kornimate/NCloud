namespace NCloud.DTOs
{
    /// <summary>
    /// Class to wrap result of command action execution for client side commands and return it to JS calls
    /// </summary>
    public class CommandDTO
    {
        public bool IsClientSideCommand { get; set; } = false;
        public string ActionHTMLElement { get; set; } = String.Empty;
        public string ActionHTMLElementId { get; set; } = String.Empty;
        public bool NoErrorWithSyntax { get; set; } = false;
        public string ErrorMessage { get; set; } = String.Empty;
    }
}
