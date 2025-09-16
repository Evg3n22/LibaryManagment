using LibaryManagment.Models;
using Microsoft.EntityFrameworkCore;

namespace LibaryManagment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StudentModel> Students { get; set; }
    }
}