using Castle.Core;
using NCloud.Models;

namespace NCloud.Services
{
    public interface ICloudTerminalService
    {
        Task<(bool, string, object?, bool)> Execute(string first, List<string> second);
        List<string> GetServerSideCommands();
        List<string> GetClientSideCommands();
        List<ClientSideCommandContainer> GetClientSideCommandsObjectList();
        List<string> GetCommands();
    }
}
