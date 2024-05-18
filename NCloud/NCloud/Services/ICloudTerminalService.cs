using Castle.Core;
using NCloud.Models;

namespace NCloud.Services
{
    /// <summary>
    /// Interface to handle cloud terminal requests
    /// </summary>
    public interface ICloudTerminalService
    {
        Task<(bool, string, object?, bool)> Execute(string first, List<string> second);
        List<string> GetServerSideCommands();
        List<string> GetClientSideCommands();
        List<ClientSideCommandContainer> GetClientSideCommandsObjectList();
        List<string> GetCommands();
        Task<string> GetClientSideCommandHTMLElement(string first, List<string> second);
    }
}
