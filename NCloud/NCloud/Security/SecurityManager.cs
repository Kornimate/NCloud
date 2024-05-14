using NCloud.ConstantData;
using System.Security.AccessControl;

namespace NCloud.Security
{
    public static class SecurityManager
    {
        public static bool CheckIfFileExists(string? path)
        {
            if(String.IsNullOrWhiteSpace(path))
                return false;

            return File.Exists(path);
        }
        public static bool CheckIfDirectoryExists(string? path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return false;

            return Directory.Exists(path);
        }

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
