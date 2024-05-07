using Castle.Core;
using NCloud.ConstantData;
using NCloud.Models;
using Newtonsoft.Json.Linq;

namespace NCloud.Services
{
    public class CloudTerminalService : ICloudTerminalService
    {
        private readonly ICloudService service;
        private readonly IHttpContextAccessor httpContext;
        private readonly List<CloudCommandContainer> commands;
        public CloudTerminalService(ICloudService service, IHttpContextAccessor httpContext)
        {
            this.service = service;
            this.httpContext = httpContext;

            commands = new List<CloudCommandContainer>() //configure commands for terminal
            {
                new CloudCommandContainer("cd",1,true, async (List<string> parameters) => await service.ChangeToDirectory(parameters[0])),
                new CloudCommandContainer("ls",0,false, async (List<string> parameters) => await service.ListCurrentSubDirectories()),
                new CloudCommandContainer("copy-file",1,false, async (List<string> parameters) => await service.CopyFile(parameters[0],parameters[1],httpContext.HttpContext!.User)),
                new CloudCommandContainer("copy-dir",1,false, async (List<string> parameters) => await service.CopyFolder(parameters[0],parameters[1],httpContext.HttpContext!.User)),
            };
        }

        public async Task<(bool, string, string, bool)> Execute(string command, List<string> parameters)
        {
            CloudCommandContainer? commandData = commands.FirstOrDefault(x => x.Command == command);

            if (commandData is null)
                throw new InvalidDataException($"no command with name: {command}");

            if (commandData.Parameters != parameters.Count)
                throw new InvalidDataException("wrong number of parameters");

            try
            {
                string result = await commandData.ExecutionAction(parameters);

                return (true, "command executed successfully", result, commandData.PrintResult);
            }
            catch (Exception)
            {
                return (false, "error while executing command", String.Empty, false);
            }
        }
    }
}
