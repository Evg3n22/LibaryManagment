// using Microsoft.EntityFrameworkCore;
// using LibaryManagment.Models;
//
// namespace LibaryManagment.Data
// {
//     public class ApplicationDbContext : DbContext
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//             : base(options)
//         {
//         }
//
//         public DbSet<AuthorModel> Authors { get; set; }
//         public DbSet<BookModel> Books { get; set; }           // Add this
//         public DbSet<BorrowingModel> Borrowings { get; set; }
//         public DbSet<LibrarianModel> Libraians { get; set; }
//         public DbSet<LoginModel> Login { get; set; }
//         public DbSet<BorrowingModel> Borrowings { get; set; }
//
//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);
//
//             // Configure AuthorModel
//             modelBuilder.Entity<AuthorModel>(entity =>
//             {
//                 entity.HasKey(e => e.AuthorID);
//                 
//                 entity.Property(e => e.AuthorName)
//                     .IsRequired()
//                     .HasMaxLength(200);
//                 
//                 entity.Property(e => e.Country)
//                     .HasMaxLength(100);
//                 
//                 entity.Property(e => e.AuthorDescription)
//                     .HasMaxLength(2000);
//
//                 // ImageFile should not be mapped to database
//                 entity.Ignore(e => e.ImageFile);
//             });
//         }
//     }
// }


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

        // DbSets
        public DbSet<AuthorModel> Authors { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<StudentModel> Students { get; set; }
        public DbSet<LibrarianModel> Librarians { get; set; }
        public DbSet<BorrowingModel> Borrowings { get; set; }
        public DbSet<QueueModel> BookQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AuthorModel
            modelBuilder.Entity<AuthorModel>(entity =>
            {
                entity.ToTable("Author");
                entity.HasKey(e => e.AuthorID);
                
                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Country)
                    .HasMaxLength(100);
                
                entity.Property(e => e.AuthorDescription)
                    .HasMaxLength(2000);

                entity.Property(e => e.AuthorImage)
                    .HasColumnType("LONGBLOB");
                entity.Ignore(e => e.ImageFile);
            });

            // Configure BookModel
            modelBuilder.Entity<BookModel>(entity =>
            {
                entity.ToTable("Books");
                entity.HasKey(e => e.BookId);
                
                entity.Property(e => e.BookName)
                    .IsRequired()
                    .HasMaxLength(300);
                
                entity.Property(e => e.AuthorName)
                    .HasMaxLength(200);
                
                entity.Property(e => e.Publisher)
                    .HasMaxLength(200);
                
                entity.Property(e => e.BookDescription)
                    .HasMaxLength(2000);
                
                entity.Property(e => e.CategoryName)
                    .HasMaxLength(100);

                entity.Property(e => e.BookImage)
                    .HasColumnType("LONGBLOB");
                entity.Ignore(e => e.ImageFile);

                // Foreign key to Author
                entity.HasOne<AuthorModel>()
                    .WithMany()
                    .HasForeignKey(e => e.AuthorID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Foreign key to Category
                entity.HasOne<CategoryModel>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CategoryModel
            modelBuilder.Entity<CategoryModel>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryID);
                
                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.CategoryDescription)
                    .HasMaxLength(500);
            });

            // Configure StudentModel
            modelBuilder.Entity<StudentModel>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(e => e.StudentId);
                
                entity.Property(e => e.StudentName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Phone)
                    .HasMaxLength(20);
                
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .HasDefaultValue("user");

                // Unique constraint on Email
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure LibrarianModel
            modelBuilder.Entity<LibrarianModel>(entity =>
            {
                entity.ToTable("Librarians");
                entity.HasKey(e => e.LibrarianId);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Phone)
                    .HasMaxLength(20);
                
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .HasDefaultValue("lib");
            });

            // Configure BorrowingModel
            modelBuilder.Entity<BorrowingModel>(entity =>
            {
                entity.ToTable("Borrowings");
                entity.HasKey(e => e.BorrowingId);
                
                entity.Property(e => e.StudentName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.BookName)
                    .IsRequired()
                    .HasMaxLength(300);
                
                entity.Property(e => e.BorrowDate)
                    .IsRequired();
                
                entity.Property(e => e.ReturnedDate)
                    .IsRequired(false);
            });

            // Configure QueueModel
            modelBuilder.Entity<QueueModel>(entity =>
            {
                entity.ToTable("BookQueue");
                entity.HasKey(e => e.QueueId);
                
                entity.Property(e => e.StudentName)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.BookName)
                    .IsRequired()
                    .HasMaxLength(300);
                
                entity.Property(e => e.RequestDate)
                    .IsRequired();
            });
        }
    }
}