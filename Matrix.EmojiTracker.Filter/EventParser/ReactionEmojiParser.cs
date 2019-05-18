using System.Collections.Generic;
using Matrix.EmojiTracker.Common.Emoji;
using Matrix.EmojiTracker.Common.MatrixEvent;
using Matrix.EmojiTracker.Database.SynapseModels;

namespace Matrix.EmojiTracker.Filter.EventParser
{
    public static class ReactionEmojiParser
    {
        public static IEnumerable<KeyValuePair<string, int>> Parse(EventJson ev)
        {
            var mtxEvent = new MatrixEvent(ev.Json);
            if (!mtxEvent.HasRelationship) return EmojiEventParser.NoEmojiCollection;

            var relationship = mtxEvent.Relationship;
            if (!relationship.IsV2Relation ||
                relationship.RelationType != EventRelationship.Annotation)
                return EmojiEventParser.NoEmojiCollection;

            return relationship.AggregationKey.GetEmoji();
        }
    }
}
