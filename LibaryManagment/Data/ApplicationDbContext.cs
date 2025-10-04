using Microsoft.EntityFrameworkCore;
using LibaryManagment.Models;

namespace LibaryManagment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuthorModel> Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AuthorModel
            modelBuilder.Entity<AuthorModel>(entity =>
            {
                entity.HasKey(e => e.AuthorID);
                
                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Country)
                    .HasMaxLength(100);
                
                entity.Property(e => e.AuthorDescription)
                    .HasMaxLength(2000);

                // ImageFile should not be mapped to database
                entity.Ignore(e => e.ImageFile);
            });
        }
    }
}