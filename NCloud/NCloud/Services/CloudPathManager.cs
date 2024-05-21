using NCloud.ConstantData;
using NCloud.Services.Exceptions;

namespace NCloud.Services
{
    /// <summary>
    /// Class to get original names in path
    /// </summary>
    public class CloudPathManager
    {
        /// <summary>
        /// Static method to get folders real name from physical file system (case sensitive way)
        /// </summary>
        /// <param name="path">Path in app</param>
        /// <param name="physicalPathStart">Physical path to beginning of app path</param>
        /// <returns>The path with original folder names in it</returns>
        /// <exception cref="CloudFunctionStopException">Throws if path is too short</exception>
        public static Task<string> GetOriginalPath(string path, string physicalPathStart)
        {
            string[] paths = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            if (paths.Length < 2)
                throw new CloudFunctionStopException("error while adjusting path");

            string[] realNames = new string[paths.Length - 1];

            realNames[0] = paths[1];

            for (int i = 2; i < paths.Length; i++)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(physicalPathStart, Path.Combine(realNames[..(i - 1)])));

                if (dirInfo.Exists)
                {
                    realNames[i - 1] = new string(dirInfo.GetDirectories().First(x => x.Name.ToLower() == paths[i].ToLower()).Name);
                }
            }

            return Task.FromResult<string>(Path.Combine(Constants.PrivateRootName, Path.Combine(realNames)));
        }
    }
}
