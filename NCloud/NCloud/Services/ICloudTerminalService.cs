using Castle.Core;

namespace NCloud.Services
{
    public interface ICloudTerminalService
    {
        Task<(bool, string, string, bool)> Execute(string first, List<string> second);
        List<string> GetServerSideCommands();
    }
}
