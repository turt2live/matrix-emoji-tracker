using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matrix.EmojiTracker.Common.PubSub;

namespace Matrix.EmojiTracker.WebWorker.Services
{
    public static class EmojiCache
    {
        private static readonly ConcurrentDictionary<string, long> CountCache = new ConcurrentDictionary<string, long>();
        private static readonly ConcurrentDictionary<string, int> Updates = new ConcurrentDictionary<string, int>();

        static EmojiCache()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var updates = Updates.ToArray();
                    Updates.Clear();

                    WebSocketTracker.BroadcastEmojiChanges(updates);
                    await Task.Delay(10);
                }
            });
        }

        public static void SetValue(string emoji, long amount)
        {
            CountCache.AddOrUpdate(emoji, amount, (k, v) => amount);
        }

        public static void HandleIncrement(IncrementCommand command)
        {
            CountCache.AddOrUpdate(command.Emoji, command.Amount, (key, existing) => existing + command.Amount);
            Updates.AddOrUpdate(command.Emoji, command.Amount, (key, existing) => existing + command.Amount);
        }

        public static Dictionary<string, long> Map()
        {
            return CountCache.ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
