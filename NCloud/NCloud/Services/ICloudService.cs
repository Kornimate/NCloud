using NCloud.Models;
using NCloud.Users;
using System.Security.Claims;

namespace NCloud.Services
{
    public interface ICloudService
    {
        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectToApp">Parameter if listed folders are only connected to app</param>
        /// <param name="connectToWeb">Parameter if listed folders are only connected to web/param>
        /// <param name="pattern">Pattern to get only folders matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string cloudPath, bool connectToApp = false, bool connectToWeb = false, string? pattern = null);

        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectToApp">Parameter if listed files are only connected to app</param>
        /// <param name="connectToWeb">Parameter if listed files are only connected to web/param>
        /// <param name="pattern">Pattern to get only files matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFile>> GetCurrentDepthCloudFiles(string cloudPath, bool connectToApp = false, bool connectToWeb = false, string? pattern = null);
        
        /// <summary>
        /// Method to get admin user from database
        /// </summary>
        /// <returns>The admin if there is admin in database</returns>
        Task<CloudUser?> GetAdmin();

        /// <summary>
        /// Method to create base folder and system folder for user
        /// </summary>
        /// <param name="cloudUser">The user</param>
        /// <returns>Boolean value if creating directory was successful</returns>
        Task<bool> CreateBaseDirectoryForUser(CloudUser cloudUser);

        /// <summary>
        /// Method to delete the root folder of the user and database items
        /// </summary>
        /// <param name="privateFolderPath">The path to the root folder of the user</param>
        /// <param name="cloudUser">The user itself from the database</param>
        /// <returns>Boolean value if method was successful</returns>
        Task<bool> DeleteDirectoriesForUser(string privateFolderPath, CloudUser cloudUser);

        /// <summary>
        /// Method to create physical folder for user
        /// </summary>
        /// <param name="folderName">Name of the folder to be created</param>
        /// <param name="cloudPath">Path to the folder in app</param>
        /// <param name="user">The owner of the folder</param>
        /// <returns>The name of the created folder</returns>
        Task<string> CreateDirectory(string folderName, string cloudPath, CloudUser user);

        /// <summary>
        /// Method to create physical file for user
        /// </summary>
        /// <param name="file">IFormFile from controller</param>
        /// <param name="cloudPath">Path to the file in app</param>
        /// <param name="user">The owner of the file</param>
        /// <returns>The name of the created file</returns>
        Task<string> CreateFile(IFormFile file, string cloudPath, CloudUser user);

        /// <summary>
        /// Method to remove physical folder form user space
        /// </summary>
        /// <param name="folderName">Name of the folder to be removed</param>
        /// <param name="cloudPath">Path to the folder in app</param>
        /// <param name="user">The owner of folder</param>
        /// <returns>Boolean indicating the success of action</returns>
        Task<bool> RemoveDirectory(string folderName, string cloudPath, CloudUser user);

        /// <summary>
        /// Method to remove physical folder form user space
        /// </summary>
        /// <param name="fileName">Name of the folder to be removed</param>
        /// <param name="cloudPath">Path to the folder in app</param>
        /// <param name="user">The owner of folder</param>
        /// <returns>Boolean indicating the success of action</returns>
        Task<bool> RemoveFile(string fileName, string cloudPath, CloudUser user);

        /// <summary>
        /// Method to return full path of physical objects (files/folders)
        /// </summary>
        /// <param name="cloudPath">Path in the app</param>
        /// <returns>The full physical path of file/folder</returns>
        string ServerPath(string cloudPath);

        /// <summary>
        /// Changes root name of path
        /// </summary>
        /// <param name="path">Path in app or shared path in app</param>
        /// <returns>The changed path , changes : shared -> cloud and vice versa</returns>
        string ChangeRootName(string path);

        /// <summary>
        /// Method to get CloudFolder object by full path
        /// </summary>
        /// <param name="cloudPath">Path in app to the folder</param>
        /// <param name="folderName">Name of the folder</param>
        /// <returns>CloudFolder object with physical data in it</returns>
        Task<DirectoryInfo> GetFolderByPath(string cloudPath, string folderName);

        /// <summary>
        /// Method to handle directory connection in database
        /// </summary>
        /// <param name="cloudPath">Path to folder in app</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="user">Owner of the folder</param>
        /// <returns>Boolean indication the success of activity</returns>
        Task<bool> ConnectDirectoryToWeb(string cloudPath, string directoryName, CloudUser user);

        /// <summary>
        /// Method to handle directory connection in database
        /// </summary>
        /// <param name="cloudPath">Path to folder in app</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="user">Owner of the folder</param>
        /// <returns>Boolean indication the success of activity</returns>
        Task<bool> ConnectDirectoryToApp(string cloudPath, string directoryName, CloudUser user);

        /// <summary>
        /// Method to handle directory connection in database
        /// </summary>
        /// <param name="cloudPath">Path to folder in app</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="user">Owner of the folder</param>
        /// <returns>Boolean indication the success of activity</returns>
        Task<bool> DisconnectDirectoryFromApp(string cloudPath, string directoryName, CloudUser user);

        /// <summary>
        /// Method to handle directory connection in database
        /// </summary>
        /// <param name="cloudPath">Path to folder in app</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="user">Owner of the folder</param>
        /// <returns>Boolean indication the success of activity</returns>
        Task<bool> DisconnectDirectoryFromWeb(string cloudPath, string directoryName, CloudUser user);

        /// <summary>
        /// Method to share file in app
        /// </summary>
        /// <param name="cloudPath">Path to file in app</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Booelan value indicating the success of action</returns>
        Task<bool> ConnectFileToApp(string cloudPath, string fileName, CloudUser user);

        /// <summary>
        /// Method to share file on web
        /// </summary>
        /// <param name="cloudPath">Path to file on web</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Booelan value indicating the success of action</returns>
        Task<bool> ConnectFileToWeb(string cloudPath, string fileName, CloudUser user);

        /// <summary>
        /// Method to stop sharing file in app
        /// </summary>
        /// <param name="cloudPath">Path to file in app</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Booelan value indicating the success of action</returns>
        Task<bool> DisconnectFileFromApp(string cloudPath, string fileName, CloudUser user);

        /// <summary>
        /// Method to stop sharing file on web
        /// </summary>
        /// <param name="cloudPath">Path to file on web</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Booelan value indicating the success of action</returns>
        Task<bool> DisconnectFileFromWeb(string cloudPath, string fileName, CloudUser user);

        /// <summary>
        /// Method to get current sharing state files
        /// </summary>
        /// <param name="sharedPath">Path in app (shared path)</param>
        /// <returns>List of CloudFiles from current sharing state</returns>
        Task<List<CloudFile>> GetCurrentDepthAppSharingFiles(string sharedPath);

        /// <summary>
        /// Method to get current sharing state folders
        /// </summary>
        /// <param name="sharedPath">Path in app (shared path)</param>
        /// <returns>List of CloudFolders from current sharing state</returns>
        Task<List<CloudFolder>> GetCurrentDepthAppSharingDirectories(string sharedPath);
        Task<bool> OwnerOfPathIsActualUser(string currentPath, ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFolderUrls(ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFileUrls(ClaimsPrincipal userPrincipal);
        Task<string> WebBackCheck(string path);
        Task<List<CloudFile>> GetCurrentDepthWebSharingFiles(string path);
        Task<List<CloudFolder>> GetCurrentDepthWebSharingDirectories(string path);
        Task<string?> CreateZipFile(List<string> itemsForDownload, string currentPath, string filePath, bool connectedToApp, bool connectedToWeb);
        Task<string> ChangePathStructure(string currentPath);
        Task<bool> ModifyFileContent(string file, string content);

        /// <summary>
        /// Method to get CloudFolder object from a specified physical folder
        /// </summary>
        /// <param name="cloudPath">Path in the app</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>CloudFolder object with specified folder data in it</returns>
        Task<CloudFolder> GetFolder(string cloudPath, string folderName);
        Task<CloudFile> GetFile(string currentPath, string fileName);

        /// <summary>
        /// Method to rename a folder
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="folderName">Original name of folder</param>
        /// <param name="newName">User defined new name of folder</param>
        /// <returns>The renamed folder name</returns>
        Task<string> RenameFolder(string cloudPath, string folderName, string newName);
        Task<string> RenameFile(string currentPath, string fileName, string newName);
        Task<string> CopyFile(string? source, string destination, ClaimsPrincipal userPrincipal);
        Task<string> CopyFolder(string? source, string destination, ClaimsPrincipal userPrincipal);
        Task<CloudPathData> GetSessionCloudPathData();
        Task<bool> SetSessionCloudPathData(CloudPathData pathData);
        Task<string> ChangeToDirectory(string path);
        Task<string> ListCurrentSubDirectories();
        Task<string> GetTerminalHelpText();
        Task<string> PrintWorkingDirectory();
        Task<List<CloudFolder>> SearchDirectoryInCurrentDirectory(string currentPath, string pattern);
        Task<List<CloudFile>> SearchFileInCurrentDirectory(string currentPath, string pattern);
    }
}
