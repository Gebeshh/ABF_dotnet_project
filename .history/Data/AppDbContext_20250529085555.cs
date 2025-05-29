using Microsoft.EntityFrameworkCore;
using TestNo_9999999.Models;

namespace TestNo_9999999.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Example DbSet properties. Replace with your actual entities.
        public DbSet<AtikBildirimFormu> AtikBildirimFormu { get; set; }
        // Add more DbSet<T> as needed
    }
}