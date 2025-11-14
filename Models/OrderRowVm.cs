namespace CLDV6212_POE.Models.ViewModels
{
    public class OrderRowVm
    {
        public Guid Id { get; set; }
        public string Customer { get; set; } = "";
        public string Products { get; set; } = "";
        public DateTimeOffset Created { get; set; }

        public string DisplayId => Id.ToString("N").Substring(0, 8);
    }
}
