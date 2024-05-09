namespace NCloud.ViewModels
{
    public class TerminalViewModel
    {
        public string? CurrentDirectory { get; set; }
        public List<string> ClientSideCommands { get; set; } = new();
        public List<string> ServerSideCommands { get; set; } = new();
    }
}
