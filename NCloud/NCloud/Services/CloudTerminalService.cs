using Castle.Core;
using NCloud.DTOs;

namespace NCloud.Services
{
    public class CloudTerminalService : ICloudTerminalService
    {
        private readonly Dictionary<string, Action> commands = new Dictionary<string, Action>()
        {
            //TODO: add command and names
        };

        public async Task<Pair<bool, string>> Execute(string first, List<string> second)
        {
            
        }
    }
}
