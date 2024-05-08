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
        private readonly List<CloudCommandContainer> serverSideCommands;
        private readonly List<CloudCommandContainer> clientSideCommands;
        public CloudTerminalService(ICloudService service, IHttpContextAccessor httpContext)
        {
            this.service = service;
            this.httpContext = httpContext;

            serverSideCommands = new List<CloudCommandContainer>() //configure commands for terminal
            {
                new CloudCommandContainer("cd",1,false, async (List<string> parameters) => await service.ChangeToDirectory(parameters[0])),
                new CloudCommandContainer("copy-file",1,true, async (List<string> parameters) => await service.CopyFile(await RelativeToAbsolutePath(parameters[0]),await RelativeToAbsolutePath(parameters[1]),httpContext.HttpContext!.User)),
                new CloudCommandContainer("copy-dir",1,true, async (List<string> parameters) => await service.CopyFolder(await RelativeToAbsolutePath(parameters[0]),await RelativeToAbsolutePath(parameters[1]),httpContext.HttpContext!.User)),
                new CloudCommandContainer("ls",0,true, async (List<string> parameters) => await service.ListCurrentSubDirectories()),
                new CloudCommandContainer("ls",0,true, async (List<string> parameters) => await service.ListCurrentSubDirectories()),
            };

            clientSideCommands = new List<CloudCommandContainer>() { };
        }

        public async Task<(bool, string, string, bool)> Execute(string command, List<string> parameters)
        {
            CloudCommandContainer? commandData = serverSideCommands.FirstOrDefault(x => x.Command == command);

            if (commandData is null)
                throw new InvalidDataException($"no command with name: {command}");

            if (commandData.Parameters != parameters.Count)
                throw new InvalidDataException("wrong number of parameters");

            try
            {
                string result = await commandData.ExecutionAction(parameters);

                return (true, Constants.TerminalGreenText("command executed successfully"), result, commandData.PrintResult);
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException(ex.Message);
            }
            catch (Exception)
            {
                return (false, "Error while executing command", String.Empty, false);
            }
        }

        public List<string> GetServerSideCommands()
        {
            return serverSideCommands.Select(x => x.Command).ToList();
        }

        public List<string> GetClientSideCommands()
        {
            return serverSideCommands.Select(x => x.Command).ToList();
        }

        public List<string> GetCommands()
        {
            return GetServerSideCommands().Union(GetClientSideCommands()).ToList();
        }

        private async Task<string> RelativeToAbsolutePath(string path)
        {
            if (path.StartsWith(Constants.AbsolutePathMarker))
                return path;

            return Path.Combine((await service.GetSessionCloudPathData()).CurrentPath,path);
        }
    }
}
