using Microsoft.EntityFrameworkCore;
using xpgp.Models;

namespace xpgp
{
	public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<KeyPair> KeyPairs { get; set; }
        public DbSet<SavedKeyPair> SavedKeyPairs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyPair>()
                .HasOne(kp => kp.User)
                .WithMany(u => u.KeyPairs)
                .HasForeignKey(kp => kp.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.PinnedKeyPair);

            modelBuilder.Entity<SavedKeyPair>()
                .HasOne(skp => skp.User);

            modelBuilder.Entity<SavedKeyPair>()
                .HasOne(skp => skp.KeyPair);

            modelBuilder.Entity<SavedKeyPair>()
                .HasOne(skp => skp.Notification);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.AssociatedUser);
        }
    }
}