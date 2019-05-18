using Matrix.EmojiTracker.Database.WorkerModels;
using Microsoft.EntityFrameworkCore;

namespace Matrix.EmojiTracker.Database
{
    public class WorkerDbContext : DbContext
    {
        // We need to define a connection string for EF Core migrations to work
        private readonly string _connString =
            "Username=worker_user;Password=YourPasswordHere;Host=localhost;Database=dont_use_synapse;";

        public DbSet<EmojiCount> EmojiCounts { get; set; }

        public WorkerDbContext(string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString)) _connString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EmojiCount>()
                .HasKey(e => new {e.Emoji, e.Source});
        }
    }
}
