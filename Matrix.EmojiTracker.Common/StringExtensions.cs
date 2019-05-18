using System;

namespace Matrix.EmojiTracker.Common
{
    public static class StringExtensions
    {
        public static int CountOccurrences(this string input, string substring)
        {
            string test = input;
            int c = 0;
            int i = 0;
            while ((i = test.IndexOf(substring, i, StringComparison.Ordinal)) != -1)
            {
                i += substring.Length;
                c++;
            }
            return c;
        }
    }
}
