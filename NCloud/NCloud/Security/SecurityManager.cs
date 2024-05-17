using NCloud.ConstantData;
using System.Security.AccessControl;

namespace NCloud.Security
{
    /// <summary>
    /// Class to ensure constraints in cloud app methods and functions
    /// </summary>
    public static class SecurityManager
    {
        /// <summary>
        /// Methdo to check if file exists
        /// </summary>
        /// <param name="path">Physical path to file</param>
        /// <returns>Boolean if file exists</returns>
        public static bool CheckIfFileExists(string? path)
        {
            if(String.IsNullOrWhiteSpace(path))
                return false;

            return File.Exists(path);
        }

        /// <summary>
        /// Methdo to check if folder exists
        /// </summary>
        /// <param name="path">Physical path to folder</param>
        /// <returns>Boolean if folder exists</returns>
        public static bool CheckIfDirectoryExists(string? path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return false;

            return Directory.Exists(path);
        }

        /// <summary>
        /// Static method to specifiy file rights when saved to server (no execution right)
        /// </summary>
        /// <returns>FileSecurity object with specified rights (read, write, no execute)</returns>
        public static FileSecurity GetFileRights()
        {
            var fileSecurity = new FileSecurity();

            var readRule = new FileSystemAccessRule(Constants.AppName, FileSystemRights.ReadData, AccessControlType.Allow);
            var writeRule = new FileSystemAccessRule(Constants.AppName, FileSystemRights.WriteData, AccessControlType.Allow);
            var noExecRule = new FileSystemAccessRule(Constants.AppName, FileSystemRights.ExecuteFile, AccessControlType.Deny); //execution denied
            
            fileSecurity.AddAccessRule(readRule);
            fileSecurity.AddAccessRule(writeRule);
            fileSecurity.AddAccessRule(noExecRule);

            return fileSecurity;
        }
    }
}
