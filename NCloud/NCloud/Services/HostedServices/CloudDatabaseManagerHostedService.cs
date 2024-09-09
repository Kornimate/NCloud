using NCloud.ConstantData;

namespace NCloud.Services.HostedServices
{
    /// <summary>
    /// Hosted service class 
    /// </summary>
    public class CloudDatabaseManagerHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<CloudDatabaseManagerHostedService> logger;
        private readonly ICloudService service;
        private Timer? timer = null;

        public CloudDatabaseManagerHostedService(ILogger<CloudDatabaseManagerHostedService> logger, ICloudService serivce)
        {
            this.logger = logger;
            this.service = serivce;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Database clean up started. [CloudDatabaseManagerHostedService]");

            timer = new Timer(DatabaseManagement, null, TimeSpan.Zero, Constants.DirectoryManagementTimeSpan);

            return Task.CompletedTask;
        }

        private void DatabaseManagement(object? state)
        {
            logger.LogInformation("Database clean up is in progress. [CloudDatabaseManagerHostedService]");

            try
            {
                service.RemoveOldLogins().Wait();
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{ex.Message}. [CloudDatabaseManagerHostedService]");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Database clean is stopping. [CloudDatabaseManagerHostedService]");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
