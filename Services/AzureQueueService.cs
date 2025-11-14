using Azure.Storage.Queues;

namespace CLDV6212_POE.Services
{
    public class AzureQueueService
    {
        private readonly IConfiguration _config;
        public AzureQueueService(IConfiguration config) => _config = config;

        public QueueServiceClient? GetClient()
        {
            var conn = _config["AzureStorageConnectionString"]
                       ?? _config.GetConnectionString("AzureStorageConnectionString");
            if (string.IsNullOrWhiteSpace(conn) || conn.Contains("DefaultEndpointsProtocol=http...")) return null;
            try { return new QueueServiceClient(conn); } catch { return null; }
        }
    }
}
