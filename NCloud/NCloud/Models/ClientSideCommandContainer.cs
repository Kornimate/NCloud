namespace NCloud.Models
{
    /// <summary>
    /// Class to store info about client side commands
    /// </summary>
    public class ClientSideCommandContainer
    {
        public string Command { get; private set; }
        public int Parameters { get; private set; }
        public bool Downloadable { get; private set; }
        public Func<List<string>,CloudPathData, Task<UrlGenerationResult>> UrlDetailsGenerator { get; private set; }
        public ClientSideCommandContainer(string command, int parameters, bool downloadable, Func<List<string>, CloudPathData, Task<UrlGenerationResult>> urlDetailsGenerator)
        {
            Command = command;
            Parameters = parameters;
            Downloadable = downloadable;
            UrlDetailsGenerator = urlDetailsGenerator;
        }
    }
}
