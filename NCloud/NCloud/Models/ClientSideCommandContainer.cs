namespace NCloud.Models
{
    public class ClientSideCommandContainer
    {
        public string Command { get; private set; }
        public int Parameters { get; private set; }

        public ClientSideCommandContainer(string command, int parameters)
        {
            Command = command;
            Parameters = parameters;
        }
    }
}
