namespace CLDV6212_POE.Models
{
    public class FileItemVm
    {
        public string Name { get; set; } = string.Empty;
        public long Length { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string? Url { get; set; }
    }
}
