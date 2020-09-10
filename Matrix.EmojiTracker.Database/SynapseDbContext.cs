using Matrix.EmojiTracker.Database.SynapseModels;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;

namespace Matrix.EmojiTracker.Database
{
    public class SynapseDbContext : DbContext
    {
        private readonly string _connString;
        public static string DefaultConnectionString { get; set; }
        private static readonly SerilogLoggerFactory LoggerFactory = new SerilogLoggerFactory(Log.ForContext<SynapseDbContext>());

        public DbSet<EventJson> EventsJson { get; set; }

        public SynapseDbContext() : this(DefaultConnectionString) { }

        public SynapseDbContext(string connectionString)
        {
            _connString = connectionString;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connString).UseLoggerFactory(LoggerFactory);
        }
    }
}
