using Microsoft.EntityFrameworkCore;

namespace IKBExpenseDb.Models
{
    public class AppDbContext : DbContext    
    {
        public DbSet<Expense> Expenses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating (ModelBuilder modelBuilder) { }

    }
}
