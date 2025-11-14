using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public int Quantity { get; set; } = 0;

        public string? ImageUrl { get; set; }
    }
}
