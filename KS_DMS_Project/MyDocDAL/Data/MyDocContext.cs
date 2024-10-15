using Microsoft.EntityFrameworkCore;
using MyDocDAL.Entities;

namespace MyDocDAL.Data
{
    public sealed class MyDocContext(DbContextOptions<MyDocContext> options) : DbContext(options)
    {
        public DbSet<MyDoc>? MyDocItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manuelle Konfiguration der Tabelle
            modelBuilder.Entity<MyDoc>(entity =>
            {
                entity.ToTable("MyDocItems");  // Setzt den Tabellennamen

                entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(100);  // Konfiguriert den "Author"-Spalten

                entity.Property(e => e.Titel)
                   .IsRequired()
                   .HasMaxLength(100);  // Konfiguriert den "Titel"-Spalten

                entity.Property(e => e.Textfield)
                 .IsRequired()
                 .HasMaxLength(100);  // Konfiguriert den "Textfield"-Spalten
                                      // entity.Property(e => e.IsComplete);  // Konfiguriert die "IsComplete"-Spalte
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}