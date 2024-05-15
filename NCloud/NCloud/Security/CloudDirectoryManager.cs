using NCloud.ConstantData;

namespace NCloud.Security
{
    public class CloudDirectoryManager
    {
        public static bool RemoveOutdatedItems(IWebHostEnvironment env)
        {
            bool everyFileDeleted = true;

            string tempfolder = Constants.GetTempFileDirectory();

            if (!Directory.Exists(tempfolder))
            {
                Directory.CreateDirectory(tempfolder);
            }

            foreach (string file in Directory.EnumerateFiles(tempfolder))
            {
                try
                {
                    FileInfo fi = new FileInfo(file);

                    if (fi.Exists && (DateTime.UtcNow - fi.CreationTimeUtc) > Constants.TempFileDeleteTimeSpan)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    everyFileDeleted = everyFileDeleted && false;
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
