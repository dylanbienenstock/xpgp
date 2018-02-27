using Microsoft.EntityFrameworkCore;
using xpgp.Models;

namespace xpgp
{
	public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<User> KeyPairs { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyPair>()
                .HasOne(kp => kp.User)
                .WithMany(u => u.KeyPairs);
        }
    }
}