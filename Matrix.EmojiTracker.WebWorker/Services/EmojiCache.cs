using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Matrix.EmojiTracker.Common.PubSub;

namespace Matrix.EmojiTracker.WebWorker.Services
{
    public static class EmojiCache
    {
        private static readonly ConcurrentDictionary<string, long> CountCache = new ConcurrentDictionary<string, long>();

        public static void SetValue(string emoji, long amount)
        {
            CountCache.AddOrUpdate(emoji, amount, (k, v) => amount);
        }

        public static void HandleIncrement(IncrementCommand command)
        {
            CountCache.AddOrUpdate(command.Emoji, command.Amount, (key, existing) => existing + command.Amount);
            WebSocketTracker.BroadcastEmojiChange(command.Emoji, command.Amount);
        }

        public static Dictionary<string, long> Map()
        {
            return CountCache.ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
