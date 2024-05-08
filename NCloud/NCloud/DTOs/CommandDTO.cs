namespace NCloud.DTOs
{
    public class CommandDTO
    {
        public bool IsClientSide { get; set; }
        public bool NoErrorWithSyntax { get; set; }
        public string ErrorMessage { get; set; }
    }
}
