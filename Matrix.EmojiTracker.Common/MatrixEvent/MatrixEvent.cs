using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common.MatrixEvent
{
    public class MatrixEvent
    {
        protected JObject _baseEvent;

        public string Origin
        {
            get => ReadBase<string>("origin");
            set => WriteBase("origin", value);
        }

        public string RoomId
        {
            get => ReadBase<string>("room_id");
            set => WriteBase("room_id", value);
        }

        public string Sender
        {
            get => ReadBase<string>("sender");
            set => WriteBase("sender", value);
        }

        public string EventId
        {
            get => ReadBase<string>("event_id");
            set => WriteBase("event_id", value);
        }

        public string StateKey
        {
            get => ReadBase<string>("state_key");
            set => WriteBase("state_key", value);
        }

        public long OriginServerTs
        {
            get => ReadBase<long>("origin_server_ts");
            set => WriteBase("origin_server_ts", value);
        }

        public JObject Content
        {
            get => ReadBase<JObject>("content");
            set => WriteBase("content", value);
        }

        public string EventType
        {
            get => ReadBase<string>("type");
            set => WriteBase("type", value);
        }

        public bool IsState => _baseEvent.ContainsKey("state_key");

        public MatrixEvent() :this(new JObject()) { }

        public MatrixEvent(JObject baseJson)
        {
            _baseEvent = baseJson;
        } 

        protected T ReadBase<T>(string key, JObject obj=null)
        {
            if (obj == null) obj = _baseEvent;
            return obj.GetValue(key).ToObject<T>();
        }

        protected void WriteBase(string key, object value, JObject obj = null)
        {
            if (obj == null) obj = _baseEvent;
            if (obj.ContainsKey(key)) obj.Property(key).Remove();
            obj.AddAfterSelf(new JProperty(key, value));
        }
    }
}
