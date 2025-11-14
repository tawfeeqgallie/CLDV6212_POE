using Azure.Storage.Blobs;

namespace CLDV6212_POE.Services
{
    public class AzureBlobService
    {
        private readonly IConfiguration _config;
        public AzureBlobService(IConfiguration config) => _config = config;

        public BlobServiceClient? GetClient()
        {
            var conn = _config["AzureStorageConnectionString"]
                       ?? _config.GetConnectionString("AzureStorageConnectionString");
            if (string.IsNullOrWhiteSpace(conn) || conn.Contains("DefaultEndpointsProtocol=http...")) return null;
            try { return new BlobServiceClient(conn); } catch { return null; }
        }
    }
}
