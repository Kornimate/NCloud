using Castle.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NCloud.ConstantData;
using NCloud.Models;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace NCloud.Services
{
    public class CloudTerminalService : ICloudTerminalService
    {
        private readonly ICloudService service;
        private readonly IHttpContextAccessor httpContext;
        private readonly List<ServerSideCommandContainer> serverSideCommands;
        private readonly List<ClientSideCommandContainer> clientSideCommands;
        private readonly IUrlHelper urlGenerator;
        public CloudTerminalService(ICloudService service, IHttpContextAccessor httpContext, IUrlHelper urlGenerator)
        {
            this.service = service;
            this.httpContext = httpContext;
            this.urlGenerator = urlGenerator;

            serverSideCommands = new List<ServerSideCommandContainer>() //configure commands for terminal
            {
                new ServerSideCommandContainer("cd",1,false,true, async (List<string> parameters) => await service.ChangeToDirectory(await PathNormalizationForChangingDirectory(parameters[0]))),
                new ServerSideCommandContainer("copy-file",2,false,true, async (List<string> parameters) => await service.CopyFile(await RelativeToAbsolutePathAndNormalize(parameters[0]),await RelativeToAbsolutePathAndNormalize(parameters[1]),httpContext.HttpContext!.User)),
                new ServerSideCommandContainer("copy-dir",2,false,true, async (List<string> parameters) => await service.CopyFolder(await RelativeToAbsolutePathAndNormalize(parameters[0]),await RelativeToAbsolutePathAndNormalize(parameters[1]),httpContext.HttpContext!.User)),
                new ServerSideCommandContainer("help",0,true,false, async (List<string> parameters) => await service.GetTerminalHelpText()),
                new ServerSideCommandContainer("ls",0,true,false, async (List<string> parameters) => await service.ListCurrentSubDirectories()),
                new ServerSideCommandContainer("pwd",0,true,false, async (List<string> parameters) => await service.PrintWorkingDirectory()),
                new ServerSideCommandContainer("mkdir",1,false,true, async (List<string> parameters) => await service.CreateDirectory(parameters[0],(await service.GetSessionCloudPathData()).CurrentPath, httpContext.HttpContext!.User)),
                new ServerSideCommandContainer("noshare-file-web",1,false,true, async (List<string> parameters) => (await service.DisconnectFileFromWeb((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("noshare-file-app",1,false,true, async (List<string> parameters) => (await service.DisconnectFileFromApp((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("noshare-dir-web",1,false,true, async (List<string> parameters) => (await service.DisconnectDirectoryFromWeb((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("noshare-dir-app",1,false,true, async (List<string> parameters) => (await service.DisconnectDirectoryFromApp((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("share-file-web",1,false,true, async (List<string> parameters) => (await service.ConnectFileToWeb((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("share-file-app",1,false,true, async (List<string> parameters) => (await service.ConnectFileToApp((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("share-dir-web",1,false,true, async (List<string> parameters) => (await service.ConnectDirectoryToWeb((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("share-dir-app",1,false,true, async (List<string> parameters) => (await service.ConnectDirectoryToApp((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("rm-dir",1,false,true, async (List<string> parameters) => (await service.RemoveDirectory(parameters[0],(await service.GetSessionCloudPathData()).CurrentPath, httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("rm-file",1,false,true, async (List<string> parameters) => (await service.RemoveFile(parameters[0],(await service.GetSessionCloudPathData()).CurrentPath, httpContext.HttpContext!.User)).ToString()),
                new ServerSideCommandContainer("rename-dir",2,false,true, async (List<string> parameters) => (await service.RenameFolder((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], parameters[1]))),
                new ServerSideCommandContainer("rename-file",2,false,true, async (List<string> parameters) => (await service.RenameFile((await service.GetSessionCloudPathData()).CurrentPath, parameters[0], parameters[1]))),
                new ServerSideCommandContainer("search-dir",1,true,true, async (List<string> parameters) => (await service.SearchDirectoryInCurrentDirectory((await service.GetSessionCloudPathData()).CurrentPath, parameters[0]))),
                new ServerSideCommandContainer("search-file",1,true,true, async (List<string> parameters) => (await service.SearchFileInCurrentDirectory((await service.GetSessionCloudPathData()).CurrentPath, parameters[0]))),
            };

            clientSideCommands = new List<ClientSideCommandContainer>()
            {
                new ClientSideCommandContainer("download-file",1,true,async (List<string> parameters) => await Task.FromResult<string>(urlGenerator.Action("DownloadFile","Drive", new { fileName = parameters[0]}) ?? throw new Exception("Invalid url"))),
                new ClientSideCommandContainer("download-dir",1,true,async (List<string> parameters) => await Task.FromResult<string>(urlGenerator.Action("DownloadFolder","Drive", new { folderName = parameters[0]}) ?? throw new Exception("Invalid url"))),
                new ClientSideCommandContainer("edit",1,false,async (List<string> parameters) => urlGenerator.Action("EditorHub","Editor", new { fileName = parameters[0], path=(await service.GetSessionCloudPathData()).CurrentPath, redirectData = RedirectionManager.CreateRedirectionString("Terminal","Index") }) ?? throw new Exception("Invalid url")),
            };
        }

        public async Task<(bool, string, object?, bool)> Execute(string command, List<string> parameters)
        {
            ServerSideCommandContainer? commandData = serverSideCommands.FirstOrDefault(x => x.Command == command);

            if (commandData is null)
                throw new InvalidDataException($"no command with name: {command}");

            if (commandData.Parameters != parameters.Count)
                throw new InvalidDataException("wrong number of parameters");

            try
            {
                object result = await commandData.ExecutionAction(parameters);

                if (bool.TryParse(result?.ToString(), out bool success))
                {
                    if (success)
                    {
                        return (true, Constants.TerminalGreenText("command executed successfully"), String.Empty, commandData.PrintResult);
                    }
                    else
                    {
                        throw new Exception("command failed");
                    }
                }

                return (true, Constants.TerminalGreenText("command executed successfully"), result, commandData.PrintResult);
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException(ex.Message);
            }
            catch (Exception)
            {
                return (false, "error while executing command", String.Empty, false);
            }
        }

        public List<string> GetServerSideCommands()
        {
            return serverSideCommands.Select(x => x.Command).ToList();
        }

        public List<string> GetClientSideCommands()
        {
            return clientSideCommands.Select(x => x.Command).ToList();
        }

        public List<string> GetCommands()
        {
            return GetServerSideCommands().Union(GetClientSideCommands()).ToList();
        }

        private async Task<string> RelativeToAbsolutePathAndNormalize(string path)
        {
            path = CloudTerminalTokenizationManager.NormalizeCommandPath(path);

            CloudPathData pathData = await service.GetSessionCloudPathData();

            if (path.StartsWith(Constants.AbsolutePathMarker) && path.StartsWith(Constants.PrivateRootName))
                return pathData.AddUserInfoToAbsolutePath(path);
            else if ((path.StartsWith(Constants.AbsolutePathMarker) && !path.StartsWith(Constants.PrivateRootName)))
                throw new InvalidDataException("wrong root name");

            return Path.Combine(pathData.CurrentPath, path);
        }

        private async Task<string> PathNormalizationForChangingDirectory(string path)
        {
            path = CloudTerminalTokenizationManager.NormalizeCommandPath(path);

            CloudPathData pathData = await service.GetSessionCloudPathData();

            if (path.StartsWith(Constants.AbsolutePathMarker) && path.StartsWith(Constants.PrivateRootName))
                return pathData.AddUserInfoToAbsolutePath(path);
            else if ((path.StartsWith(Constants.AbsolutePathMarker) && !path.StartsWith(Constants.PrivateRootName)))
                throw new InvalidDataException("wrong root name");

            return path;
        }

        public List<ClientSideCommandContainer> GetClientSideCommandsObjectList()
        {
            return clientSideCommands;
        }

        public async Task<string> GetClientSideCommandHTMLElement(string command, List<string> parameters)
        {
            var commandObject = clientSideCommands.FirstOrDefault(x => x.Command == command);

            string url = await (commandObject?.UrlGenerator(parameters) ?? throw new InvalidDataException($"no command with name: {command}"));

            return $"<div style=\"display:none\"><a href=\"{url}\" {(commandObject.Downloadable ? "download" : "")} id=\"{Constants.DownloadHTMLElementId}\"></a></div>";
        }
    }
}
