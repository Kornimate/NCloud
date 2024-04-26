using NCloud.ConstantData;

namespace NCloud.Security
{
    public class CloudDirectoryManager
    {
        public static bool RemoveOutdatedItems(IWebHostEnvironment env)
        {
            bool everyFileDeleted = true;

            string tempfolder = Path.Combine(env.WebRootPath, Constants.TempFolderName);

            if (!Directory.Exists(tempfolder))
            {
                Directory.CreateDirectory(tempfolder);
            }

            foreach (string file in Directory.EnumerateFiles(tempfolder))
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

            if (!Directory.Exists(tempfolder))
            {
                Directory.CreateDirectory(tempfolder);
            }

            return everyFileDeleted;
        }
    }
}
