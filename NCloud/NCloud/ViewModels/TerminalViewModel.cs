namespace NCloud.ViewModels
{
    public class TerminalViewModel
    {
        public string? CurrentDirectory { get; set; }
        public List<string> Commands { get; set; } = new();
    }
}
