namespace NCloud.DTOs
{
    public class CommandDTO
    {
        public bool IsClientSide { get; set; } = false;
        public string CommandName { get; set; } = String.Empty;
        public List<string> Parameters { get; set; } = new();
        public bool NoErrorWithSyntax { get; set; } = false;
        public string ErrorMessage { get; set; } = String.Empty;
    }
}
