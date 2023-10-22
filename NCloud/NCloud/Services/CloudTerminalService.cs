namespace NCloud.Services
{
    public class CloudTerminalService : ICloudTerminalService
    {
        private readonly Dictionary<string, Action> commands = new Dictionary<string, Action>()
        {
            //TODO: add command and names
        };
        public string ExecuteCommand(string? input)
        {
            //TODO: implement command choosing and error handling
            return "Hello Terminal!";
        }
    }
}
