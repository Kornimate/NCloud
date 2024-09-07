using NCloud.Models;
using NCloud.Users;
using System.Text;

namespace NCloud.Services
{
    /// <summary>
    /// Interface to manage database and physical file operations
    /// </summary>
    public interface ICloudService
    {
        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectedToApp">Parameter if listed folders are only connected to app</param>
        /// <param name="connectedToWeb">Parameter if listed folders are only connected to web/param>
        /// <param name="pattern">Pattern to get only folders matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string cloudPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null);

        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectedToApp">Parameter if listed files are only connected to app</param>
        /// <param name="connectedToWeb">Parameter if listed files are only connected to web/param>
        /// <param name="pattern">Pattern to get only files matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFile>> GetCurrentDepthCloudFiles(string cloudPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null);

        /// <summary>
        /// Method to get admin user from database
        /// </summary>
        /// <returns>The admin if there is admin in database</returns>
        Task<CloudUser?> GetAdmin();

        /// <summary>
        /// Method to remove user from database
        /// </summary>
        /// <param name="user">User to be removed</param>
        /// <returns>Boolean indication the success of action</returns>
        Task<bool> RemoveUser(CloudUser user);

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
        /// <param name="physicalPath">Physical path to folder on disk</param>
        /// <param name="folderName">Name of the folder</param>
        /// <returns>CloudFolder object with physical data in it</returns>
        Task<DirectoryInfo> GetFolderByPath(string physicalPath, string folderName);

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

        /// <summary>
        /// Method to decide if logged in user is owner of path
        /// </summary>
        /// <param name="sharedPath">Pat in app (shared path)</param>
        /// <param name="user">Logged in user</param>
        /// <returns></returns>
        Task<bool> OwnerOfPathIsActualUser(string sharedPath, CloudUser user);

        /// <summary>
        /// Method to get every shared folder for user
        /// </summary>
        /// <param name="user">The logged in user</param>
        /// <returns>List of web shared folders wrapped in SharedFolder object</returns>
        Task<List<SharedFolder>> GetUserWebSharedFolders(CloudUser user);

        /// <summary>
        /// Method to get every shared file for user
        /// </summary>
        /// <param name="user">The logged in user</param>
        /// <returns>List of web shared files wrapped in SharedFile object</returns>
        Task<List<SharedFile>> GetUserWebSharedFiles(CloudUser user);

        /// <summary>
        /// Method to check if Back action is possible on web sharing path
        /// </summary>
        /// <param name="path">Web sharing path</param>
        /// <returns>New modified path (if possible Back action the reduced path, otherwise the original)</returns>
        Task<string> WebBackCheck(string path);

        /// <summary>
        /// Method to get current web sharing state files
        /// </summary>
        /// <param name="path">Web sharing path</param>
        /// <returns>List of CloudFile objects with current web sharing state files</returns>
        Task<List<CloudFile>> GetCurrentDepthWebSharingFiles(string path);


        /// <summary>
        /// Method to get current web sharing state folders
        /// </summary>
        /// <param name="path">Web sharing path</param>
        /// <returns>List of CloudFolder objects with current web sharing state folders</returns>
        Task<List<CloudFolder>> GetCurrentDepthWebSharingDirectories(string path);

        /// <summary>
        /// Method to create zip file on server
        /// </summary>
        /// <param name="itemsForDownload">List of names to be zipped</param>
        /// <param name="cloudPath">Path in app for source of zip file</param>
        /// <param name="filePath">output zip file name and path</param>
        /// <param name="connectedToApp">Filter for app shared files and folders</param>
        /// <param name="connectedToWeb">Filter for web shared files and folders</param>
        /// <returns></returns>
        Task<string?> CreateZipFile(List<string> itemsForDownload, string cloudPath, string filePath, bool connectedToApp, bool connectedToWeb);
        /// <summary>
        /// Method to change from sharing to cloud path and vice versa
        /// </summary>
        /// <param name="path">Sharing or cloud path</param>
        /// <returns>The modified path or the original if path is too short or error occurs</returns>
        Task<string> ChangePathStructure(string path);

        /// <summary>
        /// Method to overwrite file content
        /// </summary>
        /// <param name="file">Cloud path to file</param>
        /// <param name="content">content to write into the file</param>
        /// <param name="user">Currently logged in user</param>
        /// <returns>Boolean value indicating the success of action</returns>
        Task<bool> ModifyFileContent(string file, string content, Encoding encoding, CloudUser user);

        /// <summary>
        /// Method to get CloudFolder object from a specified physical folder
        /// </summary>
        /// <param name="cloudPath">Path in the app</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>CloudFolder object with specified folder data in it</returns>
        Task<CloudFolder> GetFolder(string cloudPath, string folderName);

        /// <summary>
        /// Method to get CloudFile object from a specified physical file
        /// </summary>
        /// <param name="cloudPath">Path in the app</param>
        /// <param name="fileName">Name of file</param>
        /// <returns>CloudFile object with specified file data in it</returns>
        Task<CloudFile> GetFile(string cloudPath, string fileName);

        /// <summary>
        /// Method to rename a folder
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="folderName">Original name of folder</param>
        /// <param name="newName">User defined new name of folder</param>
        /// <param name="sharedData">Actual path in app sharing state</param>
        /// <returns>The renamed folder name</returns>
        Task<string> RenameFolder(string cloudPath, string folderName, string newName, SharedPathData sharedData);

        /// <summary>
        /// Method to rename a file
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="fileName">Original name of file</param>
        /// <param name="newName">User defined new name of file</param>
        /// <returns>The renamed file name</returns>
        Task<string> RenameFile(string cloudPath, string fileName, string newName);

        /// <summary>
        /// Method to copy a file physically
        /// </summary>
        /// <param name="source">Path in app (path to file and file name also)</param>
        /// <param name="destination">Path in app (destination folder)</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Empty string if copied file name not changed, new name if changed during copy (might be renamed)</returns>
        Task<string> CopyFile(string source, string destination, CloudUser user);

        /// <summary>
        /// Method to copy a file physically
        /// </summary>
        /// <param name="source">Path in app (path to file and file name also)</param>
        /// <param name="destination">Path in app (destination folder)</param>
        /// <param name="user">Owner of file</param>
        /// <returns>Empty string if copied file name not changed, new name if changed during copy (might be renamed)</returns>
        Task<string> CopyFolder(string source, string destination, CloudUser user);

        /// <summary>
        /// Method to change to directory in session (relative or absolute path)
        /// </summary>
        /// <param name="path">Relative or absolut epath in app</param>
        /// <param name="pathData">Current state in session</param>
        /// <returns>The modified current state data</returns>
        Task<CloudPathData> ChangeToDirectory(string path, CloudPathData pathData);

        /// <summary>
        /// Method to list current state items
        /// </summary>
        /// <param name="pathData">Current state in session</param>
        /// <returns>The string representing the items formatted like powershell but with cloud data</returns>
        Task<string> ListCurrentSubDirectories(CloudPathData pathData);

        /// <summary>
        /// Method to get commands and their syntax
        /// </summary>
        /// <returns>A string representing the help text (commands and explanations)</returns>
        Task<string> GetTerminalHelpText();

        /// <summary>
        /// Method to print the current state in folder system
        /// </summary>
        /// <returns>The current state in file system in cloud path format</returns>
        Task<string> PrintWorkingDirectory(CloudPathData pathData);

        /// <summary>
        /// Method to filter folders as the pattern specifies
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="pattern">Pattern for filtering (*,?)</param>
        /// <returns>The list of filtered folders in list of CloudFolder objects</returns>
        Task<List<CloudFolder>> SearchDirectoryInCurrentDirectory(string cloudPath, string pattern);

        /// <summary>
        /// Method to filter files as the pattern specifies
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="pattern">Pattern for filtering (*,?)</param>
        /// <returns>The list of filtered files in list of CloudFile objects</returns>
        Task<List<CloudFile>> SearchFileInCurrentDirectory(string cloudPath, string pattern);

        /// <summary>
        /// Method to adjust storage used by user by measuring the file system of user
        /// </summary>
        /// <param name="user">The currently logged in user</param>
        /// <returns>Task for async usage</returns>
        Task CheckUserStorageUsed(CloudUser user);

        /// <summary>
        /// Method to get shared folder by id
        /// </summary>
        /// <param name="id">Id of folder</param>
        /// <returns>The path of the folder</returns>
        Task<SharedFolder> GetWebSharedFolderById(Guid id);

        /// <summary>
        /// Method to get shared file by id
        /// </summary>
        /// <param name="id">Id of file</param>
        /// <returns>The path of the folder</returns>
        Task<SharedFile> GetWebSharedFileById(Guid id);

        /// <summary>
        /// Method to get a shared folder by path in app and name
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>The data from database wrapped in SharedFolder object</returns>
        Task<SharedFolder> GetSharedFolderByPathAndName(string cloudPath, string folderName);

        /// <summary>
        /// Method to get a shared file by path in app and name
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="fileName">Name of file</param>
        /// <returns>The data from database wrapped in SharedFile object</returns>
        Task<SharedFile> GetSharedFileByPathAndName(string cloudPath, string fileName);

        /// <summary>
        /// Method to see if sharing path or the parent sharing path exists in database
        /// </summary>
        /// <param name="path">Path in current sharing state</param>
        /// <returns>Bollean indication the presenc of the path in database</returns>
        Task<bool> SharedPathExists(string sharingPath);
        Task<List<CloudUser>> GetCloudUsers();
        Task<bool> SetUserSpaceSize(Guid userId, SpaceSizes spaceSize);
        Task<CloudUser> GetUserById(Guid userId);
        Task<bool> LockOutUser(CloudUser user);
        Task<bool> EnableUser(CloudUser user);
        Task<bool> CreateNewSpaceRequest(CloudSpaceRequest spaceRequest, CloudUser? user);
    }
}
