using Microsoft.EntityFrameworkCore;

namespace TestNo_9999999.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet<T> properties go here
    }
}