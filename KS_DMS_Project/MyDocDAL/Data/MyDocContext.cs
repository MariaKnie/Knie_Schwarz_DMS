using Microsoft.EntityFrameworkCore;
using MyDocDAL.Entities;

namespace MyDocDAL.Data
{
    public class MyDocContext : DbContext
    {
        public MyDocContext(DbContextOptions<MyDocContext> options)
            : base(options)
        {
        }

        //public MyDocContext()
        //{
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("MyDocDatabase");
        //}

        public DbSet<MyDoc>? MyDocItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyDoc>(entity =>
            {
                entity.ToTable("mydocitems");

                entity.HasKey(e => e.id); // Make sure the property name matches

                entity.Property(e => e.author)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.textfield)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.filename)
                    .HasMaxLength(255);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
