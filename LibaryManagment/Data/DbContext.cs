using LibaryManagment.Models;
using Microsoft.EntityFrameworkCore;

namespace LibaryManagment.Data
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options) { }

        public DbSet<StudentModel> Students { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<LibrarianModel> Librarians { get; set; }
        public DbSet<BorrowingModel> Borrows { get; set; }
    }
}