using Matrix.EmojiTracker.Database;

namespace Matrix.EmojiTracker.Persist
{
    public class WorkerDb : WorkerDbContext
    {
        internal static string ConnectionString { get; set; }

        public WorkerDb() : base(ConnectionString) { }
    }
}
