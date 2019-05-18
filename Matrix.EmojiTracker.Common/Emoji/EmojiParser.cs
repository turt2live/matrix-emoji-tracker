using System.Collections.Generic;
using System.Linq;

namespace Matrix.EmojiTracker.Common.Emoji
{
    public static class EmojiParser
    {
        public static IEnumerable<string> GetEmoji(this string input)
        {
            return EmojiData.KnownSymbols.Where(input.Contains);
        }
    }
}
