using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common.MatrixEvent
{
    public class MatrixEvent
    {
        protected JObject _baseEvent;

        public JObject Raw => _baseEvent;

        public string Origin
        {
            get => _baseEvent.GetValue<string>("origin");
            set => _baseEvent.SetValue("origin", value);
        }

        public string RoomId
        {
            get => _baseEvent.GetValue<string>("room_id");
            set => _baseEvent.SetValue("room_id", value);
        }

        public string Sender
        {
            get => _baseEvent.GetValue<string>("sender");
            set => _baseEvent.SetValue("sender", value);
        }

        public string EventId
        {
            get => _baseEvent.GetValue<string>("event_id");
            set => _baseEvent.SetValue("event_id", value);
        }

        public bool IsState => _baseEvent.ContainsKey("state_key");

        public string StateKey
        {
            get => _baseEvent.GetValue<string>("state_key");
            set => _baseEvent.SetValue("state_key", value);
        }

        public long OriginServerTs
        {
            get => _baseEvent.GetValue<long>("origin_server_ts");
            set => _baseEvent.SetValue("origin_server_ts", value);
        }

        public JObject Content
        {
            get => _baseEvent.GetValue<JObject>("content");
            set => _baseEvent.SetValue("content", value);
        }

        public string EventType
        {
            get => _baseEvent.GetValue<string>("type");
            set => _baseEvent.SetValue("type", value);
        }

        public bool HasRelationship => Content.ContainsKey(EventRelationship.ContentKeyName);

        public EventRelationship Relationship => new EventRelationship(this);

        public MatrixEvent() :this(new JObject()) { }

        public MatrixEvent(JObject baseJson)
        {
            _baseEvent = baseJson;
        }
    }
}
