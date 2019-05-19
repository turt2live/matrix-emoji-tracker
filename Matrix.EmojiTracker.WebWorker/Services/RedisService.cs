using Matrix.EmojiTracker.Common.Emoji;
using Matrix.EmojiTracker.Common.PubSub;
using StackExchange.Redis;

namespace Matrix.EmojiTracker.WebWorker.Services
{
    public static class RedisService
    {
        private static ConnectionMultiplexer _redis;

        public static void Begin(string host, int port)
        {
             _redis = ConnectionMultiplexer.Connect($"{host}:{port}");
             _redis.GetSubscriber().Subscribe(EmojiChannel.IncrementCommands, (chan, rawJson) =>
             {
                 var command = IncrementCommand.Parse(rawJson);
                 EmojiCache.HandleIncrement(command);
             });

             // Populate the cache with all known emoji
             var db = _redis.GetDatabase();
             foreach (var emoji in EmojiData.KnownSymbols)
             {
                var countStr = db.StringGet(emoji);
                long count = string.IsNullOrWhiteSpace(countStr) ? 0 : long.Parse(countStr);
                EmojiCache.SetValue(emoji, count);
             }
        }
    }
}
