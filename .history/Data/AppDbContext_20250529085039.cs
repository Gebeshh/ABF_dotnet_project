using Microsoft.EntityFrameworkCore;

namespace TestNo_9999999.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Example DbSet properties. Replace with your actual entities.
        public DbSet<AtikBildirim> AtikBildirimFormu { get; set; }
        public DbSet<Kullanici> ErrorViewModel { get; set; }
        // Add more DbSet<T> as needed
    }
}