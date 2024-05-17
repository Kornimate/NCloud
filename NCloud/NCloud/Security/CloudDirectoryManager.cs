using NCloud.ConstantData;

namespace NCloud.Security
{
    /// <summary>
    /// Class to clean up created files for download
    /// </summary>
    public class CloudDirectoryManager
    {
        /// <summary>
        /// Static method periodically called from Program.cs by Timer to clean up remaining files from downloads
        /// </summary>
        /// <returns>Boolean value indicating the success of action</returns>
        public static bool RemoveOutdatedItems()
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
