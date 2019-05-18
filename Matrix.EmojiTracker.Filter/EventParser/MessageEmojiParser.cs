using System.Collections.Generic;
using Matrix.EmojiTracker.Common.Emoji;
using Matrix.EmojiTracker.Common.MatrixEvent;
using Matrix.EmojiTracker.Database.SynapseModels;

namespace Matrix.EmojiTracker.Filter.EventParser
{
    public static class MessageEmojiParser
    {
        public static IEnumerable<KeyValuePair<string, int>> Parse(EventJson ev)
        {
            var mtxEvent = new MessageEvent(ev.Json);

            if (mtxEvent.MessageType != MessageEventType.Text &&
                mtxEvent.MessageType != MessageEventType.Emote)
                return EmojiEventParser.NoEmojiCollection;

            return mtxEvent.Body.GetEmoji();
        }
    }
}
