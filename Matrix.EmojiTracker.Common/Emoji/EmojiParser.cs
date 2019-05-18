using System.Collections.Generic;
using System.Linq;

namespace Matrix.EmojiTracker.Common.Emoji
{
    public static class EmojiParser
    {
        public static IEnumerable<KeyValuePair<string, int>> GetEmoji(this string input)
        {
            return EmojiData.KnownSymbols.Select(s => new KeyValuePair<string, int>(s, input.CountOccurrences(s)));
        }
    }
}
