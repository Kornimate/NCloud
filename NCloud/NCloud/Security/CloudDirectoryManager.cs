using NCloud.ConstantData;

namespace NCloud.Security
{
    public class CloudDirectoryManager
    {
        public static bool RemoveOutdatedItems(IWebHostEnvironment env)
        {
            bool everyFileDeleted = true;

            foreach (string file in Directory.EnumerateFiles(Path.Combine(env.WebRootPath,Constants.TempFolderName)))
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Exists && (DateTime.UtcNow - fi.CreationTimeUtc) > Constants.TempFileDeleteTimeSpan)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                        everyFileDeleted = everyFileDeleted && false;
                    }
                }
            }

            return everyFileDeleted;
        }
    }
}
