using System;
using System.Collections.Generic;
using Matrix.EmojiTracker.Common.MatrixEvent;
using Matrix.EmojiTracker.Database.SynapseModels;

namespace Matrix.EmojiTracker.Filter.EventParser
{
    public static class EmojiEventParser
    {
        private static readonly Dictionary<string, Func<EventJson, IEnumerable<KeyValuePair<string, int>>>> Parsers =
            new Dictionary<string, Func<EventJson, IEnumerable<KeyValuePair<string, int>>>>()
            {
                {MatrixEventType.RoomMessage, MessageEmojiParser.Parse},
                {MatrixEventType.Reaction, ReactionEmojiParser.Parse}
            };

        public static readonly IEnumerable<KeyValuePair<string, int>> NoEmojiCollection = new KeyValuePair<string, int>[0];

        public static bool CanParse(string eventType)
        {
            return Parsers.ContainsKey(eventType);
        }

        public static IEnumerable<KeyValuePair<string, int>> CountEmoji(string eventType, EventJson ev)
        {
            return !Parsers.ContainsKey(eventType) ? NoEmojiCollection : Parsers[eventType](ev);
        }
    }
}
