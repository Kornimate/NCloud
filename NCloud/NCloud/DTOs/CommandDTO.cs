namespace NCloud.DTOs
{
    public class CommandDTO
    {
        public bool IsClientSide { get; set; } = false;
        public bool NoErrorWithSyntax { get; set; } = false;
        public string ErrorMessage { get; set; } = String.Empty;
    }
}
