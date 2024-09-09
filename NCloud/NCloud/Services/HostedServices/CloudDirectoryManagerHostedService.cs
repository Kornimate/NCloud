using NCloud.ConstantData;

namespace NCloud.Services.HostedServices
{
    /// <summary>
    /// Class to clean up created files for download
    /// </summary>
    public class CloudDirectoryManagerHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<CloudDirectoryManagerHostedService> logger;
        private Timer timer = null!;
        public CloudDirectoryManagerHostedService(ILogger<CloudDirectoryManagerHostedService> logger)
        {
            this.logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Directory clean up service started. [CloudDirectoryManagerHostedService]");

            timer = new Timer(ManageDirectories, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void ManageDirectories(object? state)
        {
            logger.LogInformation("Directory clean up service is in progress. [CloudDirectoryManagerHostedService]");

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

                    if (fi.Exists && DateTime.UtcNow - fi.CreationTimeUtc > Constants.DirectoryManagementTimeSpan)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                    logger.LogWarning($"Item can not be removed ({file}). [CloudDirectoryManagerHostedService]");
                }
            }

            if (!Directory.Exists(tempfolder))
            {
                Directory.CreateDirectory(tempfolder);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Directory clean up service is stopping. [CloudDirectoryManagerHostedService]");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
