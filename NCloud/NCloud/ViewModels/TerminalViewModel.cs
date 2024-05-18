namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Terminal Index action method
    /// </summary>
    public class TerminalViewModel
    {
        public string? CurrentDirectory { get; set; }
        public List<string> ClientSideCommands { get; set; } = new();
        public List<string> ServerSideCommands { get; set; } = new();
    }
}
