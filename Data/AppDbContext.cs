using CLDV6212_POE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CLDV6212_POE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        //public DbSet<Order> Orders { get; set; }
    }
}
