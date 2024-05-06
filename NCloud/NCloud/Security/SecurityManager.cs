namespace NCloud.Security
{
    public static class SecurityManager
    {
        public static bool CheckIfFileExists(string path)
        {
            return File.Exists(path);
        }
        public static bool CheckIfDirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
