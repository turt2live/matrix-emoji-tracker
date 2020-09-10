using System;
using System.Linq;
using Matrix.EmojiTracker.Common;
using Matrix.EmojiTracker.Common.PubSub;
using Matrix.EmojiTracker.Database;
using Matrix.EmojiTracker.Database.SynapseModels;
using Matrix.EmojiTracker.Filter.EventParser;
using Matrix.SynapseInterop.Replication;
using Matrix.SynapseInterop.Replication.DataRows;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;
using StreamPosition = Matrix.SynapseInterop.Replication.StreamPosition;

namespace Matrix.EmojiTracker.Filter
{
    internal class Program
    {
        private static readonly string StreamPositionCacheKey = "synapse_event_stream_position";

        private static ILogger log;
        private static IConfiguration _config;
        private static ConnectionMultiplexer _redis;
        private static IDatabase _redisDb;
        private static ReplicationStream<EventStreamRow> _eventStream;

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

            StartRedis();

            string eventStreamPosition = _redisDb.StringGet(StreamPositionCacheKey);

            // TODO: Pull events in a stream gap, if needed

            StartReplicationAsync();

            Console.ReadKey(true);
        }

        private static async void StartReplicationAsync()
        {
            var replication = new SynapseReplication();
            replication.ClientName = "EmojiTracker_FilterProc";
            replication.ServerName += Replication_ServerName;

            var synapseConfig = _config.GetSection("Synapse");

            await replication.ConnectTcp(synapseConfig.GetValue<string>("replicationHost"),
                synapseConfig.GetValue<int>("replicationPort"));

            _eventStream = replication.BindStream<EventStreamRow>();
            _eventStream.DataRow += Stream_DataRow;
            _eventStream.PositionUpdate += Stream_PositionUpdate;
        }

        private static void Stream_PositionUpdate(object sender, string e)
        {
            _redisDb.StringSet(StreamPositionCacheKey, e, flags: CommandFlags.FireAndForget);
        }

        private static void Stream_DataRow(object sender, EventStreamRow e)
        {
            if (e.Kind != EventStreamRow.RowKind.Event) return; // We don't parse state events
            if (!EmojiEventParser.CanParse(e.EventType)) return;

            EventJson ev;
            using (var db = new SynapseDbContext(_config.GetConnectionString("synapse")))
            {
                ev = db.EventsJson.FirstOrDefault(e2 => e2.RoomId == e.RoomId && e2.EventId == e.EventId);
                if (ev == null) return;
            }

            var emoji = EmojiEventParser.CountEmoji(e.EventType, ev).Where(c => c.Value > 0);
            foreach (var pair in emoji)
            {
                log.Information("Found {0} instances of {1} emoji in type {2}", pair.Value, pair.Key, e.EventType);
                _redis.GetSubscriber().PublishAsync(EmojiChannel.IncrementCommands, IncrementCommand.Make(pair.Key, pair.Value, e.EventType));
                _redisDb.StringIncrementAsync(pair.Key, pair.Value, CommandFlags.FireAndForget);
            }
        }

        private static void Replication_ServerName(object sender, string e)
        {
            log.Information("Server name: " + e);
        }

        private static void StartRedis()
        {
            var redisConfig = _config.GetSection("Redis");

            _redis = ConnectionMultiplexer.Connect(
                $"{redisConfig.GetValue<string>("host")}:{redisConfig.GetValue<int>("port")}");
            _redisDb = _redis.GetDatabase();
        }
    }
}
