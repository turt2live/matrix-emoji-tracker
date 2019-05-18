using System;
using Matrix.EmojiTracker.Common;
using Microsoft.Extensions.Configuration;
using Matrix.EmojiTracker.Common.PubSub;
using Serilog;
using StackExchange.Redis;

namespace Matrix.EmojiTracker.Persist
{
    class Program
    {
        private static ILogger log;
        private static IConfiguration _config;
        private static ConnectionMultiplexer _redis;
        private static IDatabase _redisDb;

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

            Console.ReadKey(true);
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
        }
    }
}
