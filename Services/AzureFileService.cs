using Azure.Storage.Files.Shares;

namespace CLDV6212_POE.Services
{
    public class AzureFileService
    {
        private readonly IConfiguration _config;
        public AzureFileService(IConfiguration config) => _config = config;

        public ShareServiceClient? GetClient()
        {
            var conn = _config["AzureStorageConnectionString"]
                       ?? _config.GetConnectionString("AzureStorageConnectionString");
            if (string.IsNullOrWhiteSpace(conn) || conn.Contains("DefaultEndpointsProtocol=http...")) return null;
            try { return new ShareServiceClient(conn); } catch { return null; }
        }
    }
}
