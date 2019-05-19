using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Matrix.EmojiTracker.Common;
using Matrix.EmojiTracker.Common.Emoji;
using Microsoft.Extensions.Configuration;
using Matrix.EmojiTracker.Common.PubSub;
using Matrix.EmojiTracker.Database.WorkerModels;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

namespace Matrix.EmojiTracker.Persist
{
    class Program
    {
        private static readonly BlockingCollection<IncrementCommand> IncrementQueue = new BlockingCollection<IncrementCommand>();

        private static ILogger log;
        private static IConfiguration _config;
        private static ConnectionMultiplexer _redis;
        private static IDatabase _redisDb;
        private static bool _running = true;

        static void Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.default.json", true, true)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            
            Logger.Setup(_config.GetSection("Logging"));
            log = Log.ForContext<Program>();

            log.Information("Starting...");

            StartDatabase();
            StartQueue();
            StartRedis();
            SyncRedisToDb();

            Console.ReadKey(true);
            _running = false;
            IncrementQueue.Dispose();
        }

        private static void StartDatabase()
        {
            WorkerDb.ConnectionString = _config.GetConnectionString("emoji");

            using (var context = new WorkerDb()) context.Database.Migrate();
            using (var context = new WorkerDb())
            {
                var sources = new string[]
                {
                    EmojiCount.SourceMessage,
                    EmojiCount.SourceReaction,
                    EmojiCount.SourceUnknown,
                };

                var records = context.EmojiCounts.ToArray();
                if (records.Count() == (sources.Length * EmojiData.KnownSymbols.Count()))
                {
                    log.Warning(
                        "Skipping database record update - suspecting the emoji are already present. This may cause problems");
                    return;
                }

                var changed = false;
                foreach (var emoji in EmojiData.KnownSymbols)
                {
                    foreach (var source in sources)
                    {
                        log.Information("Checking for {0}/{1} emoji...", source, emoji);
                        var record = records.FirstOrDefault(e => e.Emoji == emoji && e.Source == source);
                        if (record != null) continue;

                        context.EmojiCounts.Add(new EmojiCount(emoji, source));
                        changed = true;
                    }
                }

                if (changed) context.SaveChanges();
            }
        }

        private static void StartQueue()
        {
            Task.Factory.StartNew(() =>
            {
                while (_running)
                {
                    try
                    {
                        var cmd = IncrementQueue.Take();

                        using (var db = new WorkerDb())
                        {
                            var record = db.EmojiCounts.Find(cmd.Emoji, cmd.SourceType);
                            if (record == null)
                            {
                                log.Warning("No record for {0}/{1}", cmd.SourceType, cmd.Emoji);
                                return;
                            }

                            record.Count += cmd.Amount;
                            db.SaveChanges();
                        }

                        log.Information("Updated stored emoji");
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Failed to handle incoming command queue");
                    }
                }
            });
        }

        private static void StartRedis()
        {
            var redisConfig = _config.GetSection("Redis");

            _redis = ConnectionMultiplexer.Connect(
                $"{redisConfig.GetValue<string>("host")}:{redisConfig.GetValue<int>("port")}");
            _redisDb = _redis.GetDatabase();

            _redis.GetSubscriber().Subscribe(EmojiChannel.IncrementCommands, HandleIncrement);
        }

        private static void HandleIncrement(RedisChannel channelName, RedisValue rawJson)
        {
            var cmd = IncrementCommand.Parse(rawJson);
            log.Information("Received increment command for {0} (amount = {1}) sourced by {2}", cmd.Emoji, cmd.Amount,
                cmd.SourceType);
            IncrementQueue.Add(cmd);
        }

        private static void SyncRedisToDb()
        {
            log.Information("Synchronizing emoji with redis/database...");

            using (var db = new WorkerDb())
            {
                var allEmoji = db.EmojiCounts.ToArray();
                var mapped = allEmoji.GroupBy(e => e.Emoji).ToDictionary(e => e.Key, e => e.Sum(p => p.Count));

                var changed = false;
                foreach (var emoji in EmojiData.KnownSymbols)
                {
                    var record = allEmoji.First(e => e.Emoji == emoji && e.Source == EmojiCount.SourceUnknown);
                    var efCount = mapped.ContainsKey(emoji) ? mapped[emoji] : 0;
                    var reCountStr = _redisDb.StringGet(emoji);
                    if (string.IsNullOrWhiteSpace(reCountStr)) reCountStr = "0";

                    var reCount = long.Parse(reCountStr);

                    if (reCount == efCount) continue;

                    if (reCount < efCount)
                    {
                        // Redis is wrong - fix it
                        _redisDb.StringSet(emoji, efCount, flags: CommandFlags.FireAndForget);
                        return;
                    }

                    record.Count += (reCount - efCount);
                    changed = true;
                }

                if (changed) db.SaveChanges();
            }

            log.Information("Done synchronizing emoji with redis/database");
        }
    }
}
