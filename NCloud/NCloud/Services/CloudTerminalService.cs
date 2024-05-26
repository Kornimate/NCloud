using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services.Exceptions;
using NCloud.Users;
using System.Text.RegularExpressions;

namespace NCloud.Services
{
    /// <summary>
    /// Class to handle cloud terminal requests
    /// </summary>
    public class CloudTerminalService : ICloudTerminalService
    {
        private readonly ICloudService service;
        private readonly List<ServerSideCommandContainer> serverSideCommands;
        private readonly List<ClientSideCommandContainer> clientSideCommands;
        public CloudTerminalService(ICloudService service)
        {
            this.service = service;

            serverSideCommands = new List<ServerSideCommandContainer>() //configure commands for terminal
            {
                new ServerSideCommandContainer("cd",1,false,true, async (parameters, pathData, sharedData, user) => (await service.ChangeToDirectory(await PathNormalizationForChangingDirectory(parameters[0], pathData),pathData)).CurrentPathShow),
                new ServerSideCommandContainer("copy-file",2,false,true, async (parameters, pathData, sharedData, user) => await service.CopyFile(await RelativeToAbsolutePathAndNormalize(parameters[0],pathData),await RelativeToAbsolutePathAndNormalize(parameters[1], pathData),user)),
                new ServerSideCommandContainer("copy-dir",2,false,true, async (parameters, pathData, sharedData, user) => await service.CopyFolder(await RelativeToAbsolutePathAndNormalize(parameters[0], pathData),await RelativeToAbsolutePathAndNormalize(parameters[1], pathData),user)),
                new ServerSideCommandContainer("help",0,true,false, async (parameters, pathData, sharedData, user) => await service.GetTerminalHelpText()),
                new ServerSideCommandContainer("ls",0,true,false, async (parameters, pathData, sharedData, user) => await service.ListCurrentSubDirectories(pathData)),
                new ServerSideCommandContainer("pwd",0,true,false, async (parameters, pathData, sharedData, user) => await service.PrintWorkingDirectory(pathData)),
                new ServerSideCommandContainer("mkdir",1,false,true, async (parameters, pathData, sharedData, user) => await service.CreateDirectory(parameters[0], pathData.CurrentPath, user)),
                new ServerSideCommandContainer("noshare-file-web",1,false,true, async (parameters, pathData, sharedData, user) => (await service.DisconnectFileFromWeb(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("noshare-file-app",1,false,true, async (parameters, pathData, sharedData, user) => (await service.DisconnectFileFromApp(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("noshare-dir-web",1,false,true, async (parameters, pathData, sharedData, user) => (await service.DisconnectDirectoryFromWeb(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("noshare-dir-app",1,false,true, async (parameters, pathData, sharedData, user) => (await service.DisconnectDirectoryFromApp(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("share-file-web",1,false,true, async (parameters, pathData, sharedData, user) => (await service.ConnectFileToWeb(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("share-file-app",1,false,true, async (parameters, pathData, sharedData, user) => (await service.ConnectFileToApp(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("share-dir-web",1,false,true, async (parameters, pathData, sharedData, user) => (await service.ConnectDirectoryToWeb(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("share-dir-app",1,false,true, async (parameters, pathData, sharedData, user) => (await service.ConnectDirectoryToApp(pathData.CurrentPath, parameters[0], user)).ToString()),
                new ServerSideCommandContainer("rm-dir",1,false,true, async (parameters, pathData, sharedData, user) => (await service.RemoveDirectory(parameters[0], pathData.CurrentPath, user)).ToString()),
                new ServerSideCommandContainer("rm-file",1,false,true, async (parameters, pathData, sharedData, user) => (await service.RemoveFile(parameters[0],pathData.CurrentPath, user)).ToString()),
                new ServerSideCommandContainer("rename-dir",2,false,true, async (parameters, pathData, sharedData, user) => (await service.RenameFolder(pathData.CurrentPath, parameters[0], parameters[1], sharedData))),
                new ServerSideCommandContainer("rename-file",2,false,true, async (parameters, pathData, sharedData, user) => (await service.RenameFile(pathData.CurrentPath, parameters[0], parameters[1]))),
                new ServerSideCommandContainer("search-dir",1,true,true, async (parameters, pathData, sharedData, user) => (await service.SearchDirectoryInCurrentDirectory(pathData.CurrentPath, parameters[0]))),
                new ServerSideCommandContainer("search-file",1,true,true, async (parameters, pathData, sharedData, user) => (await service.SearchFileInCurrentDirectory(pathData.CurrentPath, parameters[0]))),
            };

            clientSideCommands = new List<ClientSideCommandContainer>()
            {
                new ClientSideCommandContainer("download-file",1,true,async (parameters, pathData) => await Task.FromResult<UrlGenerationResult>(new UrlGenerationResult("Drive", "DownloadFile", new { fileName = parameters[0]}, true))),
                new ClientSideCommandContainer("download-dir",1,true,async (parameters, pathData) => await Task.FromResult<UrlGenerationResult>(new UrlGenerationResult("Drive", "DownloadFolder", new { folderName = parameters[0]}, true))),
                new ClientSideCommandContainer("edit",1,false,async (parameters, pathData) => await Task.FromResult<UrlGenerationResult>(new UrlGenerationResult("Editor", "EditorHub", new { fileName = parameters[0], path=pathData.CurrentPath, redirectData = RedirectionManager.CreateRedirectionString("Terminal","Index") }, false))),
            };
        }

        #region Public Methods
        public async Task<(bool, string, object?, bool)> Execute(string command, List<string> parameters, CloudPathData pathdata, SharedPathData sharedData, CloudUser user)
        {
            ServerSideCommandContainer? commandData = serverSideCommands.FirstOrDefault(x => x.Command == command);

            if (commandData is null)
                throw new CloudFunctionStopException($"no command with name: {command}");

            if (commandData.Parameters != parameters.Count)
                throw new CloudFunctionStopException("wrong number of parameters");

            try
            {
                object result = await commandData.ExecutionAction(parameters, pathdata, sharedData, user); //here comes the execution of actions from CloudService

                if (bool.TryParse(result?.ToString(), out bool success))
                {
                    if (success)
                    {
                        return (true, "command executed successfully", String.Empty, commandData.PrintResult);
                    }
                    else
                    {
                        throw new Exception("command failed");
                    }
                }

                return (true, "command executed successfully", result, commandData.PrintResult);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
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

        public List<ClientSideCommandContainer> GetClientSideCommandsObjectList()
        {
            return clientSideCommands;
        }

        public async Task<UrlGenerationResult> GetClientSideCommandUrlDetails(string command, List<string> parameters, CloudPathData pathData)
        {
            var commandObject = clientSideCommands.FirstOrDefault(x => x.Command == command);

            return await (commandObject?.UrlDetailsGenerator(parameters, pathData) ?? throw new CloudFunctionStopException($"no command with name: {command}"));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Private method to create path from used written path (command line input)
        /// </summary>
        /// <param name="path">Relative path in app</param>
        /// <param name="pathData">Current state in session</param>
        /// <returns>The new path parsed by rules</returns>
        /// <exception cref="CloudFunctionStopException">Throws if invalid data is present in path</exception>
        private async Task<string> RelativeToAbsolutePathAndNormalize(string path, CloudPathData pathData)
        {
            if (path.StartsWith(Constants.AbsolutePathMarker) && !Regex.IsMatch(path, Constants.AbsolutePathRegex))
                throw new CloudFunctionStopException("absolute path contains invalid character(s)");

            else if (!Regex.IsMatch(path, Constants.RelativePathRegex))
                throw new CloudFunctionStopException("relative path contains invalid character(s)");

            path = CloudTerminalTokenizationManager.NormalizeCommandPath(path);

            if (path.StartsWith(Constants.AbsolutePathMarker) && path.StartsWith(Constants.PrivateRootName))
                return await CloudPathManager.GetOriginalPath(pathData.AddUserInfoToAbsolutePath(path), service.ServerPath(Constants.PrivateRootName));

            else if ((path.StartsWith(Constants.AbsolutePathMarker) && !path.StartsWith(Constants.PrivateRootName)))
                throw new CloudFunctionStopException("wrong root name");

            return await Task.FromResult<string>(Path.Combine(pathData.CurrentPath, path));
        }

        /// <summary>
        /// Private method to create path from used written path (command line input) for folder changing
        /// </summary>
        /// <param name="path">Relative path in app</param>
        /// <param name="pathData">Current state in session</param>
        /// <returns>The new path parsed by rules</returns>
        /// <exception cref="CloudFunctionStopException">Throws if invalid data is present in path</exception>
        private async Task<string> PathNormalizationForChangingDirectory(string path, CloudPathData pathData)
        {
            if (path.StartsWith(Constants.AbsolutePathMarker) && !Regex.IsMatch(path, Constants.AbsolutePathRegex))
                throw new CloudFunctionStopException("absolute path contains invalid character(s)");

            else if (!Regex.IsMatch(path, Constants.RelativePathRegex))
                throw new CloudFunctionStopException("relative path contains invalid character(s)");

            path = CloudTerminalTokenizationManager.NormalizeCommandPath(path);

            if (path.StartsWith(Constants.AbsolutePathMarker) && path.StartsWith(Constants.PrivateRootName))
                return await CloudPathManager.GetOriginalPath(pathData.AddUserInfoToAbsolutePath(path), service.ServerPath(Constants.PrivateRootName));

            else if ((path.StartsWith(Constants.AbsolutePathMarker) && !path.StartsWith(Constants.PrivateRootName)))
                throw new CloudFunctionStopException("wrong root name");

            return await Task.FromResult<string>(path);
        }

        #endregion
    }
}
