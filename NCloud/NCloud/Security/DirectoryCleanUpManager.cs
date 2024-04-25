using NCloud.ConstantData;

namespace NCloud.Security
{
    public class DirectoryCleanUpManager
    {
        public Task<bool> RemoveOutdatedItems()
        {
            bool everyFileDeleted = true;

            foreach (string file in Directory.EnumerateFiles(Constants.TempFilePath))
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Exists && (fi.CreationTimeUtc - DateTime.UtcNow) > Constants.TempFileDeleteTimeSpan)
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

            return Task.FromResult<bool>(everyFileDeleted);
        }
    }
}
