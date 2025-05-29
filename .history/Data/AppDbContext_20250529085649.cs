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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure AtikBildirimFormu entity
            modelBuilder.Entity<AtikBildirimFormu>(entity =>
            {
                // Add configurations like keys, relationships, indexes, etc.
                // Example: entity.HasKey(e => e.Id);
            });

            // Add more entity configurations as needed
        }
    }
}