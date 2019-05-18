using System;
using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common.MatrixEvent
{
    public class EventRelationship
    {
        public static readonly string ContentKeyName = "m.relates_to";
        public static readonly string ClassicReplyKey = "m.in_reply_to";
        public static readonly string Annotation = "m.annotation";

        private JObject _relationship;

        public bool IsClassicReply => _relationship.ContainsKey(ClassicReplyKey);
        public bool IsV2Relation => _relationship.ContainsKey("rel_type");

        public string InReplyToEventId => _relationship.GetValue<JObject>(ClassicReplyKey).GetValue<string>("event_id");
        public string RelationType => _relationship.GetValue<string>("rel_type");
        public string RelationEventId => IsV2Relation ? _relationship.GetValue<string>("event_id") : InReplyToEventId;
        public string AggregationKey => _relationship.GetValue<string>("key");

        public EventRelationship(MatrixEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            if (!ev.Content.ContainsKey(ContentKeyName))
                throw new ArgumentException("Missing relationship information");

            _relationship = ev.Content.GetValue<JObject>(ContentKeyName);
        }
    }
}
