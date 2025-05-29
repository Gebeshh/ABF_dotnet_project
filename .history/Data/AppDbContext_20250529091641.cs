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

        public DbSet<AtikBildirimFormu> AtikBildirimFormu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AtikBildirimFormu entity
            modelBuilder.Entity<AtikBildirimFormu>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.KayitNo)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.GonderenKisim)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.GonderenKisi)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.AtikIsmi)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.SapmaDkHtf)
                    .HasMaxLength(500);

                entity.Property(e => e.MiktarKg)
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Durum)
                    .HasMaxLength(100);

                entity.Property(e => e.KisimAtikSorumlusuId)
                    .HasMaxLength(100);

                entity.Property(e => e.UsmPersoneli)
                    .HasMaxLength(200);

                entity.Property(e => e.UsmPersonelId)
                    .HasMaxLength(100);
            });
        }
    }
}
