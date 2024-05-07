using Castle.Core;

namespace NCloud.Services
{
    public interface ICloudTerminalService
    {
        Task<Pair<bool, string>> Execute(string first, List<string> second);
    }
}
