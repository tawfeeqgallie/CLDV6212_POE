using Azure.Data.Tables;

namespace CLDV6212_POE.Services
{
    public class AzureTableService
    {
        private readonly IConfiguration _config;
        public AzureTableService(IConfiguration config) => _config = config;

        public TableServiceClient? GetClient()
        {
            var conn = _config["AzureStorageConnectionString"]
                       ?? _config.GetConnectionString("AzureStorageConnectionString");
            if (string.IsNullOrWhiteSpace(conn) || conn.Contains("DefaultEndpointsProtocol=http...")) return null;
            try { return new TableServiceClient(conn); } catch { return null; }
        }
    }
}
