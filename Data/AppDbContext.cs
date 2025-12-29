using Microsoft.EntityFrameworkCore;
using Taxi_API.Models;

namespace Taxi_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.DriverProfile)
                .WithOne(dp => dp.User)
                .HasForeignKey<DriverProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DriverProfile>()
                .HasMany(dp => dp.Photos)
                .WithOne(p => p.DriverProfile)
                .HasForeignKey(p => p.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}